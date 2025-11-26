using FribergCarRentalsAPI.Constants;
using FribergCarRentalsAPI.Data.Services;
using FribergCarRentalsAPI.Dto.Cars;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace FribergCarRentalsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly ICarService carService;
        private readonly IHostEnvironment hostEnvironment;

        public CarsController(ICarService carService, IHostEnvironment hostEnvironment)
        {
            this.carService = carService;
            this.hostEnvironment = hostEnvironment;
        }

        // GET /api/cars 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarViewDto>>> GetAllCars()
        {
            var cars = await carService.GetAllCarsAsync();
            return Ok(cars);
        }

        [HttpGet("periods")]
        public async Task<ActionResult<List<UnavailablePeriodDto>>> GetUnavailablePeriod(
            int id, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("Invalid date range specified.");
            }
            try
            {
                var result = await carService.GetUnavailablePeriodsAsync(id, startDate, endDate);
                return Ok(result);
            }
            catch (Exception)
            {

                throw;
            }


        }

        // GET /api/cars/search?startDate=...&endDate=...
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CarViewDto>>> GetAvailableCars(
            [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            if (startDate > endDate || startDate < DateOnly.FromDateTime(DateTime.Today))
            {
                return BadRequest("Invalid date range specified.");
            }

            var cars = await carService.GetAvailableCarsAsync(startDate, endDate);
            return Ok(cars);
        }

        // GET /api/cars/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CarViewDto>> GetCar(int id)
        {
            var car = await carService.GetCarByIdAsync(id);

            if (car == null)
            {
                return NotFound($"Car with ID {id} not found.");
            }

            return Ok(car);
        }

        [HttpGet("images")] // e.g., GET /api/cars/images
        public async Task<ActionResult<List<string>>> GetCarImageFilenames()
        {
            var webRootPath = hostEnvironment.ContentRootPath;
            var imagePath = Path.Combine(webRootPath, "Data", "images", "cars");

            if (!Directory.Exists(imagePath))
            {
                return NotFound("Car images directory not found.");
            }

            var filenames = Directory.GetFiles(imagePath)
                                    .Select(Path.GetFileName)
                                    .Where(name => name != null)
                                    .ToList();
            
            return Ok(filenames);
        }

        // POST /api/cars/admin/types
        [HttpPost("admin/types")]
        [Authorize(Roles = ApiRoles.Administrator)]
        public async Task<IActionResult> AddCarModel([FromBody] CarModelDto modelDto)
        {
            var (success, error) = await carService.AddCarModelAsync(modelDto);

            if (!success)
            {
                return Conflict(new { Message = error });
            }

            return Ok();
        }

        // PUT /api/cars/admin/types/{id}
        [HttpPut("admin/types/{id:int}")]
        [Authorize(Roles = ApiRoles.Administrator)]
        public async Task<IActionResult> UpdateCarModel(int id, [FromBody] CarModelDto modelDto)
        {
            var (success, error) = await carService.UpdateCarModelAsync(id, modelDto);

            if (!success)
            {
                if (error!.Contains("not found")) return NotFound(new { Message = error });
                if (error.Contains("already exists")) return Conflict(new { Message = error });

                return Problem(error, statusCode: 500);
            }
            return Ok(); // think standard is actually no content here
            return NoContent();
        }

        // POST /api/cars/admin/inventory
        [HttpPost("admin/inventory")]
        [Authorize(Roles = ApiRoles.Administrator)]
        public async Task<IActionResult> AddCar([FromBody] CarDto carDto)
        {
            var (success, error) = await carService.AddCarAsync(carDto);

            if (!success)
            {
                if (error!.Contains("Model ID") || error.Contains("License plate"))
                    return Conflict(new { Message = error });

                return Problem(error, statusCode: 500);
            }
            return Ok();
        }

        // PUT /api/cars/admin/inventory/{id}
        [HttpPut("admin/inventory/{id:int}")]
        [Authorize(Roles = ApiRoles.Administrator)]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] CarDto carDto)
        {
            var (success, error) = await carService.UpdateCarAsync(id, carDto);

            if (!success)
            {
                if (error!.Contains("not found")) return NotFound(new { Message = error });
                if (error.Contains("exist") || error.Contains("License plate"))
                    return Conflict(new { Message = error });

                return Problem(error, statusCode: 500);
            }
            return Ok(); // think standard is actually no content here
            return NoContent();
        }

        // DELETE /api/cars/admin/inventory/{id}
        [HttpDelete("admin/inventory/{id:int}")]
        [Authorize(Roles = ApiRoles.Administrator)]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var (success, error) = await carService.DeleteCarAsync(id);

            if (!success)
            {
                return Conflict(new { Message = error });
            }
            return Ok(); // think standard is actually no content here
            return NoContent();
        }

        // DELETE /api/cars/admin/types/{id}
        [HttpDelete("admin/types/{id:int}")]
        [Authorize(Roles = ApiRoles.Administrator)]
        public async Task<IActionResult> DeleteCarModel(int id)
        {
            var (success, error) = await carService.DeleteCarModelAsync(id);

            if (!success)
            {
                return Conflict(new { Message = error });
            }
            return Ok(); // think standard is actually no content here
            return NoContent();
        }

        // GET /api/cars/admin/types
        [HttpGet("admin/types")]
        [Authorize(Roles = ApiRoles.Administrator)]
        public async Task<ActionResult<List<CarModelDto>>> GetAllCarModels()
        {
            var carModelDtos = await carService.GetAllModelsAsync();
            return Ok(carModelDtos);
        }

    }
}
