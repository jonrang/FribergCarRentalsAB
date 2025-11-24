using FribergCarRentalsAPI.Dto.Cars;
using Microsoft.EntityFrameworkCore;

namespace FribergCarRentalsAPI.Data.Services
{
    public class CarService : ICarService
    {
        private readonly CarRentalAPIContext context;

        public CarService(CarRentalAPIContext context)
        {
            this.context = context;
        }

        public async Task<(bool Success, string? Error)> AddCarAsync(CarDto carDto)
        {
            var modelExists = await context.CarModels.AnyAsync(m => m.CarModelId == carDto.CarModelId);
            if (!modelExists)
            {
                return (false, $"Car Model ID '{carDto.CarModelId}' does not exist.");
            }

            var existingCar = await context.Cars.FirstOrDefaultAsync(c => c.LicensePlate == carDto.LicensePlate);
            if (existingCar != null)
            {
                return (false, $"Car with license plate '{carDto.LicensePlate}' already exists.");
            }

            var newCar = new Car
            {
                CarModelId = carDto.CarModelId,
                Make = carDto.Make,
                Year = carDto.Year,
                LicensePlate = carDto.LicensePlate,
                RatePerDay = carDto.RatePerDay,
                Mileage = carDto.Mileage,
                IsAvailable = true
            };

            context.Cars.Add(newCar);
            await context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Succes, string? Error)> AddCarModelAsync(CarModelDto modelDto)
        {
            var existingModel = await context.CarModels.FirstOrDefaultAsync(m => m.Name == modelDto.Name);

            if (existingModel != null)
            {
                return (false, $"A car model named '{modelDto.Name}' already exists.");
            }

            var newModel = new CarModel
            {
                Name = modelDto.Name,
                Manufacturer = modelDto.Manufacturer,
                BodyStyle = modelDto.BodyStyle,
                ImageFileName = modelDto.ImageFileName
            };

            context.CarModels.Add(newModel);
            await context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteCarAsync(int id)
        {
            var carToDelete = await context.Cars.FindAsync(id);

            if (carToDelete == null)
            {
                return (true, null); // Idempotent: it's "deleted" if it's not found
            }

            var activeRentals = await context.Rentals
                .CountAsync(r => r.CarId == id &&
                                 (r.Status == RentalStatus.Pending || r.Status == RentalStatus.Active));

            if (activeRentals > 0)
            {
                return (false, $"Cannot delete car; it has {activeRentals} active or pending rentals.");
            }

            context.Cars.Remove(carToDelete);
            await context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteCarModelAsync(int id)
        {
            var modelToDelete = await context.CarModels.FindAsync(id);

            if (modelToDelete != null)
            {
                return (false, "Car model not found");
            }

            var carsCount = await context.CarModels.CountAsync(c => c.CarModelId == id);

            if (carsCount > 0)
            {
                return (false, $"Cannot delete model; {carsCount} cars are still registered under this model.");
            }

            context.CarModels.Remove(modelToDelete);
            await context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<IEnumerable<CarViewDto>> GetAllCarsAsync()
        {
            var cars = await context.Cars
                //.Where(c => c.IsAvailable)
               .Include(c => c.Model)
               .Select(c => new CarViewDto
               {
                   CarId = c.Id,
                   LicensePlateSnippet = c.LicensePlate.Length > 4 ? c.LicensePlate.Substring(c.LicensePlate.Length - 4) : c.LicensePlate,
                   Year = c.Year,
                   RatePerDay = c.RatePerDay,
                   ModelName = c.Model.Name,
                   Manufacturer = c.Model.Manufacturer,
                   BodyStyle = c.Model.BodyStyle,
                   ImageFileName = c.Model.ImageFileName
               })
                .ToListAsync();

            return cars;
        }

        public async Task<List<CarModelDto>> GetAllModelsAsync()
        {
            var carModel = await context.CarModels
                .Select(c => new CarModelDto
                {
                    CarModelId = c.CarModelId,
                    Name = c.Name,
                    Manufacturer = c.Manufacturer,
                    BodyStyle = c.BodyStyle,
                    ImageFileName = c.ImageFileName
                })
                .ToListAsync();
            return carModel;
        }

        public async Task<IEnumerable<CarViewDto>> GetAvailableCarsAsync(DateOnly startDate, DateOnly endDate)
        {
            var unavailableCarIds = await context.Rentals
                .Where(r => r.Status == RentalStatus.Pending || r.Status == RentalStatus.Active)
                .Where(r => startDate <= r.EndDate && endDate >= r.StartDate)
                .Select(r => r.CarId)
                .Distinct()
                .ToListAsync();

            var availableCars = await context.Cars
                .Where(c => c.IsAvailable)
                .Where(c => !unavailableCarIds.Contains(c.Id))
                .Include(c => c.Model)
                .Select(c => new CarViewDto
                {
                    CarId = c.Id,
                    LicensePlateSnippet = c.LicensePlate.Length > 4 ? c.LicensePlate.Substring(c.LicensePlate.Length - 4) : c.LicensePlate,
                    Year = c.Year,
                    RatePerDay = c.RatePerDay,

                    ModelName = c.Model.Name,
                    Manufacturer = c.Model.Manufacturer,
                    BodyStyle = c.Model.BodyStyle,
                    ImageFileName = c.Model.ImageFileName
                })
                .ToListAsync();

            return availableCars;
        }

        public async Task<CarViewDto?> GetCarByIdAsync(int id)
        {
            var car = await context.Cars
                .Include(c => c.Model)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                return null;
            }

            return new CarViewDto
            {
                CarId = car.Id,
                LicensePlateSnippet = car.LicensePlate.Length > 4 ? car.LicensePlate.Substring(car.LicensePlate.Length - 4) : car.LicensePlate,
                Year = car.Year,
                RatePerDay = car.RatePerDay,

                ModelName = car.Model.Name,
                Manufacturer = car.Model.Manufacturer,
                BodyStyle = car.Model.BodyStyle,
                ImageFileName = car.Model.ImageFileName
            };
        }

        public async Task<(bool Success, string? Error)> UpdateCarAsync(int id, CarDto carDto)
        {
            var carToUpdate = await context.Cars.FindAsync(id);

            if (carToUpdate == null)
            {
                return (false, "Car instance not found.");
            }

            if (carToUpdate.CarModelId != carDto.CarModelId)
            {
                var modelExists = await context.CarModels.AnyAsync(m => m.CarModelId == carDto.CarModelId);
                if (!modelExists)
                {
                    return (false, $"New Car Model ID '{carDto.CarModelId}' does not exist.");
                }
            }

            var licenseConflict = await context.Cars
                .AnyAsync(c => c.LicensePlate == carDto.LicensePlate && c.Id != id);

            if (licenseConflict)
            {
                return (false, $"License plate '{carDto.LicensePlate}' is already assigned to another car.");
            }

            carToUpdate.CarModelId = carDto.CarModelId;
            carToUpdate.Make = carDto.Make;
            carToUpdate.Year = carDto.Year;
            carToUpdate.LicensePlate = carDto.LicensePlate;
            carToUpdate.RatePerDay = carDto.RatePerDay;
            carToUpdate.Mileage = carDto.Mileage;

            try
            {
                await context.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                return (false, "Concurrency error: The car instance was updated or deleted by another user.");
            }
        }

        public async Task<(bool Success, string? Error)> UpdateCarModelAsync(int id, CarModelDto modelDto)
        {
            var modelToUpdate = await context.CarModels.FindAsync(id);

            if (modelToUpdate == null)
            {
                return (false, "Car model not found.");
            }

            var nameConflict = await context.CarModels
                .AnyAsync(m => m.Name == modelDto.Name && m.CarModelId != id);

            if (nameConflict)
            {
                return (false, $"A car model named '{modelDto.Name}' already exists.");
            }

            modelToUpdate.Name = modelDto.Name;
            modelToUpdate.Manufacturer = modelDto.Manufacturer;
            modelToUpdate.BodyStyle = modelDto.BodyStyle;
            modelToUpdate.ImageFileName = modelDto.ImageFileName;

            try
            {
                await context.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                return (false, "Concurrency error: The model was updated or deleted by another user.");
            }
        }
    }
}
