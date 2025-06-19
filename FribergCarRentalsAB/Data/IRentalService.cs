using FribergCarRentalsAB.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FribergCarRentalsAB.Data
{
    public interface IRentalService
    {
        /// <summary>Is the car free between the given dates?</summary>
        Task<bool> IsCarAvailableAsync(int carId, DateTime start, DateTime end);

        /// <summary>Create a new rental and mark the car unavailable.</summary>
        Task<Rental> CreateRentalAsync(int userId, int carId, DateTime start, DateTime end);

        /// <summary>Compute any late‐return fees (or zero if on time).</summary>
        Task<decimal> ComputeLateFeeAsync(int rentalId);

        /// <summary>Mark a rental returned now; update cost, status, and car availability.</summary>
        Task<Rental> MarkReturnedAsync(int rentalId);

        /// <summary>Get a SelectList of cars free in a window.</summary>
        Task<SelectList> GetAvailableCarsAsync(int? carId, DateTime? start = null, DateTime? end = null);

        /// <summary>Get a SelectList of all users (admins only).</summary>
        Task<SelectList> GetUsersSelectListAsync();

        Task<IList<Rental>> GetActiveRentalsAsync(int userId);
        Task<IList<Rental>> GetOldRentalsAsync(int userId);
        Task CancelRentalAsync(int rentalId);
    }
}
