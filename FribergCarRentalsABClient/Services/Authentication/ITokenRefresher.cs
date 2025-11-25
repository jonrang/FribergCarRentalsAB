namespace FribergCarRentalsABClient.Services.Authentication
{
    public interface ITokenRefresher
    {
        Task<string?> RefreshTokensAsync();
        Task Logout();
        Task<bool> IsTokenNearExpiryAsync();
    }
}
