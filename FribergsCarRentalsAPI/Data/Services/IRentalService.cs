using System.Security.Claims;
using FribergCarRentalsAPI.Dto;

namespace FribergCarRentalsAPI.Data.Services
{
    public interface IRentalService
    {
        // Customer Methods
        Task<(bool Success, string? Error, RentalViewDto? Rental)> CreateRentalAsync(CreateRentalDto rentalDto, string userId);
        Task<IEnumerable<RentalViewDto>> GetUserRentalsAsync(string userId);
        Task<(bool Success, string? Error)> CancelRentalAsync(int rentalId, string userId);

        // Admin Methods
        Task<(bool Success, string? Error)> ActivateRentalAsync(int rentalId);
        Task<IEnumerable<RentalViewDto>> GetAllRentalsAsync();
        Task<RentalViewDto?> GetRentalByIdAsync(int rentalId);
        Task<(bool Success, string? Error)> UpdateRentalStatusAsync(int rentalId, RentalStatus newStatus);
        Task<(bool Success, string? Error, decimal FinalCost)> CompleteRentalAsync(int rentalId);
    }
}
