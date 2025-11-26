using FribergCarRentalsABClient.Models;

namespace FribergCarRentalsABClient.Services.Cars
{
    public interface ICarService
    {
        Task<List<CarModel>> GetAllCarsAsync();
        Task<List<CarModel>> GetAvailableCarsAsync(DateOnly startDate, DateOnly endDate);
        Task<CarModel> GetCarByIdAsync(int id);


        Task<bool> AddCarModelAsync(CarModel model);
        Task<bool> UpdateCarModelAsync(int id, CarModel model);
        Task<bool> DeleteCarModelAsync(int id);

        Task<bool> AddCarInventoryAsync(CarCreateDto carDto);
        Task<bool> UpdateCarInventoryAsync(int id, CarUpdateDto car, int carModelId);
        Task<bool> DeleteCarInventoryAsync(int id);
        Task<List<CarModel>> GetAllCarModelsAsync();
        Task<List<string>> GetCarImageFilenamesAsync();
    }
}