using System.Security.Claims;
using FribergCarRentalsAPI.Constants;
using FribergCarRentalsAPI.Data.Services;
using FribergCarRentalsAPI.Dto;
using FribergCarRentalsAPI.Dto.Rentals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FribergCarRentalsAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService rentalService;
        private readonly ICarService carService;

        public RentalsController(IRentalService rentalService, ICarService carService)
        {
            this.rentalService = rentalService;
            this.carService = carService;
        }

        // GET /api/rentals/me (Customer's rentals)
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<RentalViewDto>>> GetMyRentals()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null) return Unauthorized();

            var rentals = await rentalService.GetUserRentalsAsync(userId);
            return Ok(rentals);
        }

        // POST /api/rentals (Customer booking)
        [HttpPost]
        public async Task<ActionResult<Response<RentalViewDto?>>> CreateRental([FromBody] CreateRentalDto rentalDto)
        {
            var isOfAge = User.FindFirst("IsOfAge")?.Value == "True";
            var hasLicense = User.FindFirst("HasDriverLicense")?.Value == "True";

            if (!isOfAge || !hasLicense)
            {
                return BadRequest(new Response<RentalViewDto>
                { 
                    Success = false,
                    Message = "User must be of age and have a valid driver's license on file to book.",
                    Data = null
                });
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var (success, error, rentalView) = await rentalService.CreateRentalAsync(rentalDto, userId);

            if (!success)
            {
                if (error!.Contains("not found")) return NotFound(new { Message = error });
                if (error.Contains("already booked") || error.Contains("duration")) return Conflict(new { Message = error });

                return Problem(error, statusCode: 500);
            }

            return Ok(new Response<RentalViewDto>
            {
                Success = true,
                Message = "Rental creation succesful.",
                Data = rentalView
            });
        }

        // PUT /api/rentals/{id}/cancel
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelRental(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var (success, error) = await rentalService.CancelRentalAsync(id, userId);

            if (!success)
            {
                if (error!.Contains("not found")) return NotFound(new { Message = error });

                return BadRequest(new { Message = error });
            }
            return Ok(); // think standard is actually no content here
            return NoContent(); 
        }

        // GET /api/rentals/{id} 
        // Accessible by Admin (any rental) or the Rental Owner
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RentalViewDto>> GetRental(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var isAdmin = User.IsInRole(ApiRoles.Administrator);

            var rental = await rentalService.GetRentalByIdAsync(id);

            if (rental == null) return NotFound();

            if (!isAdmin && rental.UserId != userId)
            {
                return Forbid();
            }

            return Ok(rental);
        }

        // PUT /api/rentals/{id}/activate
        [HttpPut("{id:int}/activate")]
        [Authorize(Roles = ApiRoles.Administrator)]
        public async Task<IActionResult> ActivateRental(int id)
        {
            var (success, error) = await rentalService.ActivateRentalAsync(id);

            if (!success)
            {
                if (error!.Contains("not found")) return NotFound(new { Message = error });
                return BadRequest(new { Message = error });
            }
            return Ok(); // think standard is actually no content here
            return NoContent();
        }


        // GET /api/rentals/admin (Get all rentals)
        [HttpGet("admin")]
        [Authorize(Roles = ApiRoles.Administrator)]
        public async Task<ActionResult<IEnumerable<RentalViewDto>>> GetAllRentalsAdmin(string id)
        {
            var rentals = await rentalService.GetUserRentalsAsync(id);
            return Ok(rentals);
        }

        // PUT /api/rentals/{id}/complete (Admin returns a car)
        [HttpPut("{id:int}/complete")]
        [Authorize(Roles = ApiRoles.Administrator)]
        public async Task<ActionResult<Response<decimal>>> CompleteRental(int id)
        {
            var (success, error, finalCost) = await rentalService.CompleteRentalAsync(id);

            if (!success)
            {
                if (error!.Contains("not found")) return NotFound(new { Message = error });
                return Conflict(new { Message = error });
            }

            return Ok(new Response<decimal>
            {
                Success = true,
                Message = $"Rental ID {id} completed successfully. Final cost: {finalCost:C}",
                Data = finalCost
            });
        }
    }
}
