using FribergCarRentalsAB.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FribergCarRentalsAB.Data
{
    public interface IRentalService
    {
       
        Task<bool> IsCarAvailableAsync(int carId, DateTime start, DateTime end);

        Task<Rental> CreateRentalAsync(int userId, int carId, DateTime start, DateTime end);

    
        Task<decimal> ComputeLateFeeAsync(int rentalId);

        
        Task<Rental> MarkReturnedAsync(int rentalId);

        
        Task<SelectList> GetAvailableCarsAsync(int? carId, DateTime? start = null, DateTime? end = null);

        
        Task<SelectList> GetUsersSelectListAsync();

        Task<IList<Rental>> GetActiveRentalsAsync(int userId);
        Task<IList<Rental>> GetOldRentalsAsync(int userId);
        Task CancelRentalAsync(int rentalId);
    }
}
