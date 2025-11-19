using FribergCarRentalsAPI.Dto;

namespace FribergCarRentalsAPI.Data.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginUserAsync(LoginUserDto userDto);
        Task<AuthResponse?> RefreshTokenAsync(string accessToken, string refreshToken);
        Task<(bool Success, IDictionary<string, string[]> Errors)> RegisterUserAsync(UserDto userDto, string defaultRole);
    }
}
