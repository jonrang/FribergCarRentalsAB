using FribergCarRentalsAPI.Dto;

namespace FribergCarRentalsAPI.Data.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginUserAsync(LoginUserDto userDto);
        Task<AuthResponse?> RefreshTokenAsync(string accessToken, string refreshToken);
        Task<string?> GenerateEmailConfirmationTokenAsync(string userId);
        Task<(bool Success, string? ErrorMessage)> ConfirmEmailAsync(string userId, string token);
        Task<(bool Success, IDictionary<string, string[]> Errors, string userID, string token)> RegisterUserAsync(RegisterUserDto userDto, string defaultRole);
    }
}
