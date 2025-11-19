using FribergCarRentalsAPI.Constants;
using FribergCarRentalsAPI.Data.Services;
using FribergCarRentalsAPI.Dto;
using Microsoft.AspNetCore.Mvc;

namespace FribergCarRentalsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            try
            {
                var (success, errors) = await authService.RegisterUserAsync(userDto, ApiRoles.User);

                if (success)
                {
                    return CreatedAtAction(nameof(Login), new { email = userDto.Email }, null);
                }
                else
                {
                    if (errors.ContainsKey("Email") && errors["Email"].Any(e => e.Contains("exists")))
                    {
                        return Conflict(new { Message = errors["Email"].First() });
                    }

                    if (errors.ContainsKey("RoleAssignment"))
                    {
                        return Problem(errors["RoleAssignment"].First(), statusCode: 500);
                    }

                    var modelStateDictionary = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
                    foreach (var key in errors.Keys)
                    {
                        foreach (var error in errors[key])
                        {
                            modelStateDictionary.AddModelError(key, error);
                        }
                    }
                    return ValidationProblem(modelStateDictionary);
                }
            }
            catch (Exception)
            {
                return Problem($"An unexpected error occurred during registration.", statusCode: 500);
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginUserDto userDto)//  LoginRequest request
        {
            try
            {
                var response = await authService.LoginUserAsync(userDto);

                // Check if the service returned null (indicating invalid credentials)
                if (response == null)
                {
                    return Unauthorized(new { Message = "Invalid Credentials" });
                }

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Problem($"Server error during token persistence: {ex.Message}", statusCode: 500);
            }
            catch (Exception)
            {
                // Catch all other unhandled exceptions (should be logged in a real app)
                return Problem($"Something went wrong in the {nameof(Login)}", statusCode: 500);
            }

        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] AuthResponse authResponse)
        {
            if (authResponse is null ||
                string.IsNullOrWhiteSpace(authResponse.AccessToken) ||
                string.IsNullOrWhiteSpace(authResponse.RefreshToken))
            {
                return BadRequest(new { Message = "Invalid client request: Tokens missing." });
            }

            try
            {
                var response = await authService.RefreshTokenAsync(authResponse.AccessToken, authResponse.RefreshToken);

                if (response == null)
                {
                    // The service returns null if the token is invalid, used, or expired.
                    return Unauthorized(new { Message = "Invalid or expired refresh token." });
                }

                return Ok(response);
            }
            catch (Exception)
            {
                return Problem("An unexpected server error occurred during token refresh.", statusCode: 500);
            }
        }
    }
}
