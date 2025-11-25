using Blazored.LocalStorage;
using FribergCarRentalsABClient.Models;
using FribergCarRentalsABClient.Providers;
using FribergCarRentalsABClient.Services.Base;
using Microsoft.AspNetCore.Components.Authorization;

namespace FribergCarRentalsABClient.Services.Authentication
{
    public class AuthenticationService : ITokenRefresher, IAuthenticationService
    {
        private readonly ICarRentalsAPIClient apiClient;
        private readonly ILocalStorageService localStorage;
        private readonly ILogger<AuthenticationService> logger;
        private readonly ApiAuthenticationStateProvider authStateProvider;

        public AuthenticationService(
            ICarRentalsAPIClient apiClient,
            ILocalStorageService localStorage,
            AuthenticationStateProvider authStateProvider,
            ILogger<AuthenticationService> logger)
        {
            this.apiClient = apiClient;
            this.localStorage = localStorage;
            this.logger = logger;
            this.authStateProvider = (ApiAuthenticationStateProvider)authStateProvider;
        }


        public async Task<bool> Login(LoginUserDto loginModel)
        {
            try
            {
                var response = await apiClient.LoginAsync(loginModel);

                if (string.IsNullOrEmpty(response.AccessToken)) return false;

                await SaveCredentials(response);

                return true;
            }
            catch (ApiException ex)
            {
                logger.LogWarning(ex, $"Login failed. Status: {ex.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred during login.");
                return false;
            }
        }

        public async Task Logout()
        {
            await localStorage.RemoveItemAsync("accessToken");
            await localStorage.RemoveItemAsync("refreshToken");
            await localStorage.RemoveItemAsync("userId");
            await localStorage.RemoveItemAsync("ExpiryTimeKey");
            await localStorage.RemoveItemAsync("RefreshTokenExpiryKey");

            authStateProvider.NotifyUserLoggedOut();
        }

        public async Task<ResponseOfstring> Register(RegisterUserDto registerModel)
        {
            try
            {
                var response = await apiClient.RegisterAsync(registerModel);
                
                return response;
            }
            catch (ApiException ex)
            {
                return new ResponseOfstring
                {
                    Success = false,
                    Message = $"An unexpected network error occurred (Status: {ex.StatusCode})."
                };
            }
            catch (Exception ex)
            {
                return new ResponseOfstring
                {
                    Success = false,
                    Message = "An unknown client error occurred."
                };
            }
        }

        public async Task<bool> ConfimEmailAsync(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            try
            {
                await apiClient.ConfirmEmailAsync(userId, token);

                return true;
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Email confirmation failed for user {userId}. Status: {ex.StatusCode}");

                return false;
            }
        }

        public async Task<string?> RefreshTokensAsync()
        {
            try
            {
                var accessToken = await localStorage.GetItemAsStringAsync("accessToken");
                var refreshToken = await localStorage.GetItemAsStringAsync("refreshToken");

                if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(accessToken)) 
                {
                    await Logout();
                    return null;
                }

                var refreshDto = new RefreshTokenDto
                {
                    RefreshToken = refreshToken,
                    AccessToken = accessToken
                };
                var response = await apiClient.RefreshAsync(refreshDto);

                await SaveCredentials(response);

                return response.AccessToken;

            }
            catch (ApiException)
            {
                await Logout();
                return null;
            }
        }

        public async Task<bool> IsTokenNearExpiryAsync()
        {
            TimeSpan refreshBuffer = TimeSpan.FromSeconds(60);

            var expiryTime = await localStorage.GetItemAsync<DateTime?>("ExpiryTimeKey");

            if (expiryTime == null)
            {
                return true;
            }

            var nowPlusBuffer = DateTime.UtcNow.Add(refreshBuffer);


            return expiryTime.Value <= nowPlusBuffer;
        }

        private async Task SaveCredentials(AuthResponse response)
        {
            var absoluteExpiryTime = DateTime.UtcNow.AddSeconds(response.ExpiresIn);

            await localStorage.SetItemAsStringAsync("accessToken", response.AccessToken);
            await localStorage.SetItemAsStringAsync("refreshToken", response.RefreshToken);
            await localStorage.SetItemAsStringAsync("userId", response.UserId);
            await localStorage.SetItemAsync("ExpiryTimeKey", absoluteExpiryTime);
            if (response.RefreshTokenExpiry.HasValue)
            {
                await localStorage.SetItemAsync("RefreshTokenExpiryKey", response.RefreshTokenExpiry.Value);
            }

            authStateProvider.NotifyUserLoggedIn(response.AccessToken);
        }

    }


}
