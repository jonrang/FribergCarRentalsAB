using FribergCarRentalsAPI.Data.Services;
using FribergCarRentalsAPI.Dto.Users;

namespace FribergCarRentalsAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // GET /api/users/me 
        [HttpGet("me")]
        public async Task<ActionResult<CustomerProfileViewDto>> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var userView = await userService.GetUserByIdAsync(userId);

            if (userView == null)
            {
                return NotFound("User profile not found.");
            }

            var customerView = new CustomerProfileViewDto
            {
                Email = userView.Email,
                FirstName = userView.FirstName,
                LastName = userView.LastName,
                PhoneNumber = userView.PhoneNumber,
                DateOfBirth = userView.DateOfBirth,
                DriverLicenseNumber = userView.DriverLicenseNumber
            };

            return Ok(customerView);
        }

        // PUT /api/users/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] CustomerProfileUpdateDto updateDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var (success, errors) = await userService.UpdateUserProfileAsync(userId, updateDto);

            if (!success)
            {
                var modelStateDictionary = new ModelStateDictionary();
                foreach (var errorPair in errors)
                {
                    var key = errorPair.Key;
                    var errorMessages = errorPair.Value;
                    foreach (var errorMessage in errorMessages)
                    {
                        modelStateDictionary.AddModelError(key, errorMessage);
                    }
                }
                return ValidationProblem(modelStateDictionary);
            }
            return Ok(); // think standard is actually no content here
            return NoContent();
        }

        // PUT /api/users/me/password
        [HttpPut("me/password")]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordDto changeDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var (success, errors) = await userService.ChangeUserPasswordAsync(userId, changeDto);

            if (!success)
            {
                var modelStateDictionary = new ModelStateDictionary();
                foreach (var errorPair in errors)
                {
                    foreach (var errorMessage in errorPair.Value)
                    {
                        modelStateDictionary.AddModelError(errorPair.Key, errorMessage);
                    }
                }
                return ValidationProblem(modelStateDictionary);
            }
            return Ok(); // think standard is actually no content here
            return NoContent();
        }
    }
}
