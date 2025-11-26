using FribergCarRentalsABClient.Services.Base;

namespace FribergCarRentalsABClient.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly ICarRentalsAPIClient apiClient;
        private readonly ILogger<AdminService> logger;

        public AdminService(ICarRentalsAPIClient apiClient, ILogger<AdminService> logger)
        {
            this.apiClient = apiClient;
            this.logger = logger;
        }

        public async Task<List<AdminUserViewDto>> GetAllUsersAsync()
        {
            try
            {

                var users = await apiClient.UsersAllAsync();

                return users.ToList();

            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to retrieve all admin users. Status: {ex.StatusCode}");
                return new List<AdminUserViewDto>();
            }
        }

        public async Task<AdminUserViewDto?> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await apiClient.UsersGETAsync(userId);
                return user;
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404)
                {
                    logger.LogWarning($"Admin user ID {userId} not found.");
                    return null;
                }
                logger.LogError(ex, $"Failed to retrieve admin user {userId}. Status: {ex.StatusCode}");
                return null;
            }
        }


        public async Task<bool> EditUserAsync(string userId, AdminProfileUpdateDto updateDto)
        {
            try
            {
                await apiClient.UsersPUTAsync(userId, updateDto);
                return true;
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404)
                {
                    logger.LogWarning($"Attempted to edit non-existent user: {userId}.");
                }
                logger.LogError(ex, $"Failed to edit user {userId}. Status: {ex.StatusCode}");
                return false;
            }
        }

        public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
        {
            try
            {
                await apiClient.RoleAsync(userId, newRole);
                return true;
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404 || ex.StatusCode == 400)
                {
                    logger.LogWarning($"Role change failed for user {userId}. Status: {ex.StatusCode}");
                }
                logger.LogError(ex, $"Failed to change role for user {userId}. Status: {ex.StatusCode}");
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                await apiClient.UsersDELETEAsync(userId);
                return true;
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404)
                {
                    logger.LogWarning($"Attempted to delete non-existent user: {userId}");
                    return false;
                }
                logger.LogError(ex, $"Failed to delete user {userId}. Status: {ex.StatusCode}");
                return false;
            }
        }

        public async Task<List<RentalViewDto>> GetAllRentalsAsync(string id)
        {
            try
            {
                var rentals = await apiClient.AdminAsync(id);

                return rentals.ToList();
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to retrieve all admin rentals. Status: {ex.StatusCode}");
                return new List<RentalViewDto>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred in GetAllRentalsAsync.");
                return new List<RentalViewDto>();
            }
        }

        public async Task<bool> ActivateRentalAsync(int id)
        {
            try
            {
                await apiClient.ActivateAsync(id);
                return true;
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404 || ex.StatusCode == 400)
                {
                    logger.LogWarning("Activation failed for rental {RentalId}. Status: {Status}", id, ex.StatusCode);
                }
                else
                {
                    logger.LogError(ex, "Failed to activate rental {RentalId}. Status: {Status}", id, ex.StatusCode);
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred in ActivateRentalAsync.");
                return false;
            }
        }

        public async Task<double?> CompleteRentalAsync(int id)
        {
            try
            {
                var response = await apiClient.CompleteAsync(id);

                if (response != null)
                {
                    return response.Data;
                }
                logger.LogError($"API returned successful status but final cost data was missing for rental {id}.");
                return null;
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to complete rental {id}. Status: {ex.StatusCode}");
                return null;
            }
        }
        public async Task<RentalViewDto?> GetRentalByIdAsync(int id)
        {
            try
            {
                var rental = await apiClient.RentalsGETAsync(id);

                return rental;
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404)
                {
                    logger.LogWarning($"Rental with ID {id} not found.");
                    return null;
                }

                logger.LogError(ex, $"Failed to retrieve rental {id}. Status: {ex.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred in GetRentalByIdAsync.");
                return null;
            }

        }
    }
}
