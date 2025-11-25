using FribergCarRentalsAPI.Dto.Users;

namespace FribergCarRentalsAPI.Data.Services
{
    public interface IUserService
    {
        Task<IEnumerable<AdminUserViewDto>> GetAllUsersAsync();
        Task<AdminUserViewDto> GetUserByIdAsync(string userId);
        Task<(bool Success, IDictionary<string, string[]> Errors)> ChangeUserRoleAsync(string userId, string newRole);
        Task<bool> DeleteUserAsync(string userId);
        Task<(bool Success, IDictionary<string, string[]> Errors)> UpdateUserByAdminAsync(string userId, AdminProfileUpdateDto updateDto);
        Task<(bool Success, IDictionary<string, string[]> Errors)> UpdateUserProfileAsync(string userId, CustomerProfileUpdateDto updateDto);
        Task<(bool Success, IDictionary<string, string[]> Errors)> ChangeUserPasswordAsync(string userId, ChangePasswordDto changeDto);
    }
}