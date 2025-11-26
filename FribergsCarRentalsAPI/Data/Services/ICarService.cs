using System;
using FribergCarRentalsAPI.Dto.Cars;
using Microsoft.AspNetCore.Http.HttpResults;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FribergCarRentalsAPI.Data.Services
{
    public interface ICarService
    {
        Task<(bool Succes, string? Error)> AddCarModelAsync(CarModelDto modelDto);
        Task<(bool Success, string? Error)> UpdateCarModelAsync(int id, CarModelDto modelDto); 
        Task<(bool Success, string? Error)> DeleteCarModelAsync(int id); 
        Task<List<CarModelDto>> GetAllModelsAsync();
        Task<(bool Success, string? Error)> AddCarAsync(CarDto carDto);
        Task<(bool Success, string? Error)> UpdateCarAsync(int id, CarDto carDto); 
        Task<(bool Success, string? Error)> DeleteCarAsync(int id);
        Task<IEnumerable<CarViewDto>> GetAllCarsAsync(); 
        Task<CarViewDto?> GetCarByIdAsync(int id);
        Task<IEnumerable<CarViewDto>> GetAvailableCarsAsync(DateOnly start, DateOnly end);
        Task<List<UnavailablePeriodDto>> GetUnavailablePeriodsAsync(int carId, DateOnly start, DateOnly end);

    }
}
