using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace FribergCarRentalsABClient.Providers
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService localStorage;
        private readonly JwtSecurityTokenHandler jwtSecurityTokenHandler;
        private readonly ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public ApiAuthenticationStateProvider(ILocalStorageService localStorage)
        {
            this.localStorage = localStorage;
            jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var savedToken = await localStorage.GetItemAsStringAsync("accessToken");
                var savedRefreshToken = await localStorage.GetItemAsStringAsync("refreshToken"); // Need to check if a refresh token exists
                var refreshTokenExpiry = await localStorage.GetItemAsync<DateTime?>("RefreshTokenExpiryKey");

                bool isHardSessionExpired = refreshTokenExpiry == null || refreshTokenExpiry.Value < DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(savedToken) || string.IsNullOrWhiteSpace(savedRefreshToken) || isHardSessionExpired)
                {
                    await ClearCredentials();
                    return new AuthenticationState(anonymous);
                }


                var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(savedToken);

                var claimsIdentity = new ClaimsIdentity(tokenContent.Claims, "JwtAuth");
                return new AuthenticationState(new ClaimsPrincipal(claimsIdentity));
            }
            catch (Exception)
            {
                await ClearCredentials();
                return new AuthenticationState(anonymous);
            }
        }

        public void NotifyUserLoggedIn(string token)
        {
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(token);
            var claimsIdentity = new ClaimsIdentity(tokenContent.Claims, "JwtAuth");
            var authenticatedUser = new ClaimsPrincipal(claimsIdentity);

            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLoggedOut()
        {
            var authState = Task.FromResult(new AuthenticationState(anonymous));
            NotifyAuthenticationStateChanged(authState);
        }

        public async Task<string?> GetAccessToken()
        {
            return await localStorage.GetItemAsStringAsync("accessToken");
        }
        public async Task<string?> GetRefreshToken()
        {
            return await localStorage.GetItemAsStringAsync("refreshToken");
        }
        public async Task<string?> GetUserId()
        {
            return await localStorage.GetItemAsStringAsync("userId");
        }
        private async Task ClearCredentials()
        {
            await localStorage.RemoveItemAsync("accessToken");
            await localStorage.RemoveItemAsync("refreshToken");
            await localStorage.RemoveItemAsync("userId");
            await localStorage.RemoveItemAsync("ExpiryTimeKey");
            await localStorage.RemoveItemAsync("RefreshTokenExpiryKey");

        }

    }
}
