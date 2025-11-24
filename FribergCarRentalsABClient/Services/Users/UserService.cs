using FribergCarRentalsABClient.Services.Base;

namespace FribergCarRentalsABClient.Services.Users
{
    public class UserService : IUserService
    {
        private readonly ICarRentalsAPIClient apiClient;
        private readonly ILogger<UserService> logger;

        public UserService(ICarRentalsAPIClient apiClient, ILogger<UserService> logger)
        {
            this.apiClient = apiClient;
            this.logger = logger;
        }

        public async Task<CustomerProfileViewDto?> GetMyProfileAsync()
        {
            try
            {
                return await apiClient.MeGETAsync();
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to fetch user profile. Status: {ex.StatusCode}");
                return null;
            }
        }


        public async Task<bool> UpdateMyProfileAsync(CustomerProfileUpdateDto updateDto)
        {
            try
            {
                await apiClient.MePUTAsync(updateDto);
                return true;
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to update user profile. Status: {ex.StatusCode}");
                return false;
            }
        }


        public async Task<bool> ChangeMyPasswordAsync(ChangePasswordDto changeDto)
        {
            try
            {
                await apiClient.PasswordAsync(changeDto);
                return true;
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to change user password. Status: {ex.StatusCode}");
                return false;
            }
        }
    }
}
