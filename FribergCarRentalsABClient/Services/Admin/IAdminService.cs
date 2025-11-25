using FribergCarRentalsABClient.Services.Base;

namespace FribergCarRentalsABClient.Services.Admin
{
    public interface IAdminService
    {
        Task<bool> ChangeUserRoleAsync(string userId, string newRole);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> EditUserAsync(string userId, AdminProfileUpdateDto updateDto);
        Task<List<AdminUserViewDto>> GetAllUsersAsync();
        Task<AdminUserViewDto?> GetUserByIdAsync(string userId);
        Task<List<RentalViewDto>> GetAllRentalsAsync(string id);
        Task<bool> ActivateRentalAsync(int id);
        Task<double?> CompleteRentalAsync(int id);
        Task<RentalViewDto?> GetRentalByIdAsync(int id);
    }
}