using System;
using FribergCarRentalsAPI.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FribergCarRentalsAPI.Data.Services
{
    public interface ICarService
    {
        Task<(bool Succes, string? Error)> AddCarModelAsync(CarModelDto modelDto); // Creates a new entry in the CarModel table.
        Task<(bool Success, string? Error)> UpdateCarModelAsync(int id, CarModelDto modelDto); // Updates an existing car model definition.
        Task<(bool Success, string? Error)> DeleteCarModelAsync(int id); // Removes a car model(Requires checking if any Car still uses it).
        Task<(bool Success, string? Error)> AddCarAsync(CarDto carDto);// Adds a specific vehicle instance to the inventory.
        Task<(bool Success, string? Error)> UpdateCarAsync(int id, CarDto carDto); // Updates details(e.g., rate, mileage) of a specific vehicle.
        Task<(bool Success, string? Error)> DeleteCarAsync(int id);
        Task<IEnumerable<CarViewDto>> GetAllCarsAsync();  // Retrieves all available cars for browsing(often mapped to a CarViewDto).
        Task<CarViewDto?> GetCarByIdAsync(int id); //Retrieves a single car's details.
        Task<IEnumerable<CarViewDto>> GetAvailableCarsAsync(DateOnly start, DateOnly end);

    }
}
