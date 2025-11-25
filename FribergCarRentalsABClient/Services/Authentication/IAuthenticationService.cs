using FribergCarRentalsABClient.Services.Base;

namespace FribergCarRentalsABClient.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task<bool> Login(LoginUserDto loginModel);
        Task Logout();
        Task<ResponseOfstring> Register(RegisterUserDto registerModel);
        Task<bool> ConfimEmailAsync(string userId, string token);
    }
}