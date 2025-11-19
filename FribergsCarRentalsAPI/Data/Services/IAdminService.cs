using FribergCarRentalsAPI.Dto;

namespace FribergCarRentalsAPI.Data.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminUserViewDto>> GetAllUsersAsync();
        Task<ApiUser?> GetUserByIdAsync(string userId);
        Task<(bool Success, IDictionary<string, string[]> Errors)> ChangeUserRoleAsync(string userId, string newRole);
        Task<bool> DeleteUserAsync(string userId);
        Task<(bool Success, IDictionary<string, string[]> Errors)> UpdateUserDetailsAsync(string userId, UserDto updateDto);
    }
}