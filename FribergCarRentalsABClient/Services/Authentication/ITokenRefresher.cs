namespace FribergCarRentalsABClient.Services.Authentication
{
    public interface ITokenRefresher
    {
        Task<string?> RefreshTokensAsync(string expiredRefreshToken);
        Task Logout();
        Task<bool> IsTokenNearExpiryAsync();
    }
}
