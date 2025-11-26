
using FribergCarRentalsABClient.Models;
using FribergCarRentalsABClient.Services.Base;

namespace FribergCarRentalsABClient.Services.Cars
{
    public class CarService : ICarService
    {
        private readonly ICarRentalsAPIClient apiClient;
        private readonly ILogger<CarService> logger;

        public CarService(ICarRentalsAPIClient apiClient, ILogger<CarService> logger)
        {
            this.apiClient = apiClient;
            this.logger = logger;
        }

        public async Task<bool> AddCarInventoryAsync(CarCreateDto carDto)
        {
            var carToMap = new CarModel
            {
                CarModelId = carDto.CarModelId,
                Manufacturer = carDto.Make,
                Year = carDto.Year,
                LicensePlate = carDto.LicensePlate,
                RatePerDay = carDto.RatePerDay,
                Mileage = carDto.Mileage
            };

            var finalCarDto = MapToCarDto(carToMap);

            try
            {
                await apiClient.InventoryPOSTAsync(finalCarDto);
                return true;
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to add car inventory for License Plate {finalCarDto.LicensePlate}. API Status: {ex.StatusCode}");
                return false;
            }
        }

        public async Task<bool> AddCarModelAsync(CarModel model)
        {
            var modelDto = MapToCarModelDto(model);

            try
            {
                await apiClient.TypesPOSTAsync(modelDto);
                return true;
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to add new car model '{model.Name}'. Status:  {ex.StatusCode}",
                    model.Name, ex.StatusCode);
                return false;
            }
        }

        public async Task<bool> DeleteCarInventoryAsync(int id)
        {
            try
            {
                await apiClient.InventoryDELETEAsync(id);
                return true;
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to delete car inventory item with ID {id} . Status:  {ex.StatusCode}",
                    id, ex.StatusCode);
                return false;
            }
        }

        public async Task<bool> DeleteCarModelAsync(int id)
        {
            try
            {
                await apiClient.TypesDELETEAsync(id);
                return true;
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to delete car model type with ID {id}. Status: {ex.StatusCode}",
                    id, ex.StatusCode);
                return false;
            }
        }

        public async Task<List<CarModel>> GetAllCarsAsync()
        {
            try
            {
                var viewDtos = await apiClient.CarsAllAsync();

                return viewDtos.Select(MapToCarModel).ToList();
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to fetch all cars. Status: {ex.StatusCode}", ex.StatusCode);
                return new List<CarModel>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred in GetAllCarsAsync.");
                return new List<CarModel>();
            }
        }

        public async Task<List<CarModel>> GetAvailableCarsAsync(DateOnly startDate, DateOnly endDate)
        {
            try
            {
                var startDateTimeOffset = new System.DateTimeOffset(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0, System.TimeSpan.Zero);
                var endDateTimeOffset = new System.DateTimeOffset(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0, System.TimeSpan.Zero);
                var viewDtos = await apiClient.SearchAsync(startDateTimeOffset, endDateTimeOffset);

                return viewDtos.Select(MapToCarModel).ToList();
            }
            catch (ApiException ex)
            {
                logger.LogError($"API Error: {ex.StatusCode} - {ex.Response}");
                return new List<CarModel>();
            }
        }

        public async Task<CarModel> GetCarByIdAsync(int id)
        {
            try
            {
                var viewDto = await apiClient.CarsAsync(id);

                return MapToCarModel(viewDto);
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404)
                {
                    logger.LogWarning($"Car with ID {id} not found.", id);
                    return null;
                }

                logger.LogError(ex, $"Failed to fetch car with ID {id}. Status: {ex.StatusCode}", id, ex.StatusCode);
                return null;
            }
        }

        public async Task<bool> UpdateCarInventoryAsync(int id, CarUpdateDto carDto, int carModelId)
        {
            var original = await GetCarByIdAsync(id);

            if (original == null)
            {
                return false;
            }
            var carToMap = new CarModel 
            {
                Id = id,
                CarModelId = carModelId,
                LicensePlate = carDto.LicensePlate,
                Year = carDto.Year,
                RatePerDay = carDto.RatePerDay,
                Mileage = carDto.Mileage,

                Manufacturer = original.Manufacturer,
                Name = original.Name,
                BodyStyle = original.BodyStyle
            };
            var finalCarDto = MapToCarDto(carToMap);

            try
            {
                await apiClient.InventoryPUTAsync(id, finalCarDto);
                return true;
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to update car inventory with ID {id}. Status: {ex.StatusCode}");
                return false;
            }
        }

        public async Task<bool> UpdateCarModelAsync(int id, CarModel model)
        {
            var modelDto = MapToCarModelDto(model);

            try
            {
                await apiClient.TypesPUTAsync(id, modelDto);
                return true;
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, $"Failed to update car model type with ID {id} . Status:  {ex.StatusCode}");
                return false;
            }
        }

        public async Task<List<CarModel>> GetAllCarModelsAsync()
        {
            try
            {
                var modelDtos = await apiClient.TypesAllAsync(CancellationToken.None);

                return modelDtos.Select(MapCarModelDtoToCarModel).ToList();
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, "Failed to fetch all car models. API Status: {Status}", ex.StatusCode);
                return new List<CarModel>();
            }
        }

        public async Task<List<string>> GetCarImageFilenamesAsync()
        {
            try
            {
                var filenames = await apiClient.ImagesAsync();
                return filenames.ToList();
            }
            catch (ApiException ex)
            {
                logger.LogError(ex, "Failed to retrieve car image filenames from API.");
                return new List<string>();
            }
        }

        private static CarModel MapToCarModel(CarViewDto viewDto)
        {
            return new CarModel
            {
                Id = viewDto.CarId,
                LicensePlateSnippet = viewDto.LicensePlateSnippet,
                RatePerDay = viewDto.RatePerDay,
                Year = viewDto.Year,

                Name = viewDto.ModelName,
                Manufacturer = viewDto.Manufacturer,
                BodyStyle = viewDto.BodyStyle,
                ImageFileName = viewDto.ImageFileName,

                LicensePlate = viewDto.LicensePlate,
                CarModelId = viewDto.CarModelId,
                Mileage = 0
            };
        }

        private static CarModelDto MapToCarModelDto(CarModel car)
        {
            return new CarModelDto
            {
                CarModelId = car.CarModelId,
                Name = car.Name,
                Manufacturer = car.Manufacturer,
                BodyStyle = car.BodyStyle,
                ImageFileName = car.ImageFileName
            };
        }

        private static CarDto MapToCarDto(CarModel car)
        {
            return new CarDto
            {
                CarModelId = car.CarModelId,
                Make = car.Manufacturer,
                Year = car.Year,
                LicensePlate = car.LicensePlate,
                RatePerDay = car.RatePerDay,
                Mileage = car.Mileage
            };
        }
        private static CarModel MapCarModelDtoToCarModel(CarModelDto modelDto)
        {
            return new CarModel
            {
                CarModelId = modelDto.CarModelId,
                Name = modelDto.Name,
                Manufacturer = modelDto.Manufacturer,
                BodyStyle = modelDto.BodyStyle,
                ImageFileName = modelDto.ImageFileName,

                Id = 0,
                LicensePlate = string.Empty,
                Year = 0,
                RatePerDay = 0,
                Mileage = 0
            };
        }
    }
}
