using FribergCarRentalsABClient.Models;
using FribergCarRentalsABClient.Services.Base;

namespace FribergCarRentalsABClient.Services.Cars
{
    public interface ICarService
    {
        // === PUBLIC/CUSTOMER METHODS ===
        Task<List<CarModel>> GetAllCarsAsync();
        Task<List<CarModel>> GetAvailableCarsAsync(DateOnly startDate, DateOnly endDate);
        Task<CarModel?> GetCarByIdAsync(int id);

        // === ADMIN METHODS (Requires Auth) ===
        Task<bool> AddCarModelAsync(CarModel model);
        Task<bool> UpdateCarModelAsync(int id, CarModel model);
        Task<bool> DeleteCarModelAsync(int id);

        Task<bool> AddCarInventoryAsync(CarModel car);
        Task<bool> UpdateCarInventoryAsync(int id, CarModel car);
        Task<bool> DeleteCarInventoryAsync(int id);
    }
}