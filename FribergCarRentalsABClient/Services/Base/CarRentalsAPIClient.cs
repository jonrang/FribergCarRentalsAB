using System.Net.Http.Headers;
using System.Text;
using Blazored.LocalStorage;
using FribergCarRentalsABClient.Providers;
using FribergCarRentalsABClient.Services.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace FribergCarRentalsABClient.Services.Base
{
    public partial class CarRentalsAPIClient : ICarRentalsAPIClient
    {
        private readonly ILocalStorageService localStorage;
        private readonly IServiceProvider serviceProvider;

        public CarRentalsAPIClient(HttpClient httpClient,
            ILocalStorageService localStorage,
            IServiceProvider serviceProvider)
            : this(httpClient)
        {
            this.localStorage = localStorage;
            this.serviceProvider = serviceProvider;
        }

        protected Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected async Task PrepareRequestAsync(HttpClient client, 
            HttpRequestMessage request, 
            string url, 
            CancellationToken cancellationToken)
        {
            var token = await localStorage.GetItemAsStringAsync("accessToken");

            if (url.Contains("api/Auth/refresh"))
            {
                return;
            }

            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            var tokenRefresher = serviceProvider.GetRequiredService<ITokenRefresher>();

            if (await tokenRefresher.IsTokenNearExpiryAsync())
            {
                var refreshToken = await localStorage.GetItemAsStringAsync("refreshToken");

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var newAccessToken = await tokenRefresher.RefreshTokensAsync();

                    if (string.IsNullOrEmpty(newAccessToken))
                    {
                        await tokenRefresher.Logout();
                        return;
                    }
                    token = newAccessToken.ToString();
                }
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        protected async Task ProcessResponseAsync(HttpClient client, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var tokenRefresher = serviceProvider.GetRequiredService<ITokenRefresher>();
                //var refreshToken = await localStorage.GetItemAsStringAsync("refreshToken");
                //if (!string.IsNullOrEmpty(refreshToken))
                //{
                //    try
                //    {
                //        var token = await tokenRefresher.RefreshTokensAsync();
                //        return;
                //    }
                //    catch (Exception)
                //    {
                //        return;
                //    }

                //}
                await tokenRefresher.Logout();
            }

            return;
        }
    }
}
