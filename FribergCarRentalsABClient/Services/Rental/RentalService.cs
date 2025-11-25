using FribergCarRentalsABClient.Models;
using FribergCarRentalsABClient.Services.Base;

namespace FribergCarRentalsABClient.Services.Rental
{
    public class RentalService : IRentalService
    {
        private readonly ICarRentalsAPIClient apiClient;
        private readonly ILogger logger;

        public RentalService(ICarRentalsAPIClient apiClient, ILogger logger)
        {
            this.apiClient = apiClient;
            this.logger = logger;
        }
        public async Task<bool> CancelRentalAsync(int id)
        {
            try
            {
                await apiClient.CancelAsync(id);
                return true;
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404)
                {
                    logger.LogWarning($"Attempted to cancel non-existent rental: {id}");
                }
                else if (ex.StatusCode == 400)
                {
                    logger.LogWarning($"Rental {id} cannot be cancelled due to invalid state. Status: 400");
                }
                else
                {
                    logger.LogError(ex, $"Failed to cancel rental {id}. Status: {ex.StatusCode}");
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"An unexpected error occurred in CancelRentalAsync for rental {id}.");
                return false;
            }

        }

        public async Task<RentalCreationResult> CreateRentalAsync(CreateRentalDto rentalDto)
        {
            try
            {
                var apiResponse = await apiClient.RentalsPOSTAsync(rentalDto);

                if (apiResponse.Success && apiResponse.Data != null)
                {
                    return new RentalCreationResult
                    {
                        Success = true,
                        Message = apiResponse.Message ?? "Booking successful.",
                        RentalDetails = new RentalViewDto 
                        { 
                            RentalId = apiResponse.Data.RentalId,
                            CarDetails = apiResponse.Data.CarDetails,
                            StartDate = apiResponse.Data.StartDate,
                            EndDate = apiResponse.Data.EndDate,
                            Status = apiResponse.Data.Status,
                            TotalCost = apiResponse.Data.TotalCost,
                            UserEmail = apiResponse.Data.UserEmail,
                            UserId = apiResponse.Data.UserId,
                            AdditionalProperties = apiResponse.Data.AdditionalProperties
                        }
                    };
                }

                return new RentalCreationResult
                {
                    Success = false,
                    Message = apiResponse.Message ?? "Booking failed due to a business rule.",
                };
            }
            catch (ApiException ex)
            {
                return new RentalCreationResult { Success = false, Message = $"An unexpected server error occurred (Status: {ex.StatusCode})." };
            }
        }

        public async Task<List<RentalViewDto>> GetMyRentalsAsync()
        {
            try
            {
                var myRentals = await apiClient.MeAllAsync();

                return myRentals.ToList();
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to retrieve my rentals. Status: {ex.StatusCode}");
                return new List<RentalViewDto>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred in GetMyRentalsAsync.");
                return new List<RentalViewDto>();
            }
        }
    }
}
