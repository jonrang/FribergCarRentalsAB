using FribergCarRentalsABClient.Models;
using FribergCarRentalsABClient.Services.Base;

namespace FribergCarRentalsABClient.Services.Rental
{
    public interface IRentalService
    {
        Task<List<RentalViewDto>> GetMyRentalsAsync();

        Task<RentalCreationResult> CreateRentalAsync(CreateRentalDto rentalDto);
        Task<bool> CancelRentalAsync(int id);
        Task<List<UnavailablePeriodDto>> GetUnavailablePeriodsAsync(int carId);
    }
}
