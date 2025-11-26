using FribergCarRentalsAPI.Constants;
using FribergCarRentalsAPI.Data.Services;
using FribergCarRentalsAPI.Dto;
using FribergCarRentalsAPI.Dto.Users;

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

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<Response<string>>> Register([FromBody] RegisterUserDto userDto)
        {
            try
            {
                var (success, errors, userId, token) = await authService.RegisterUserAsync(userDto, ApiRoles.User);

                if (success)
                {
                    var scheme = HttpContext.Request.Scheme;
                    var host = HttpContext.Request.Host.ToUriComponent();

                    var confirmationLink = $"{scheme}://{host}/api/auth/confirm-email?userId={userId}&token={Uri.EscapeDataString(token)}";

                    return Ok(new Response<string>
                    {
                        Success = true,
                        Message = "Registration successful. Please confirm your email.",
                        Data = confirmationLink
                    });
                }
                else
                {
                    if (errors.ContainsKey("Email") && errors["Email"].Any(e => e.Contains("exists")))
                    {
                        return Conflict(new Response<string>
                        {
                            Success = false,
                            Message = errors["Email"].First()
                        });
                    }

                    if (errors.ContainsKey("RoleAssignment"))
                    {
                        return StatusCode(500, new Response<string>
                        {
                            Success = false,
                            Message = errors["RoleAssignment"].First()
                        });
                    }

                    var validationErrorDictionary = errors.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.ToArray()
                        );

                    return BadRequest(new Response<string>
                    {
                        Success = false,
                        Message = "Registration failed due to invalid data.",
                        ValidationErrors = validationErrorDictionary
                    });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new Response<string>
                {
                    Success = false,
                    Message = "An unexpected server error occurred during registration."
                });
            }
        }

        // GET /api/auth/confirm-email?userId=...&token=...
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var (success, errorMessage) = await authService.ConfirmEmailAsync(userId, token);

            if (success)
            {
                return Ok(new { Message = "Email confirmed successfully. You can now log in." });
            }

            return BadRequest(new { Message = errorMessage });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginUserDto userDto)
        {
            try
            {
                var response = await authService.LoginUserAsync(userDto);

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
        [AllowAnonymous]
        [Route("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenDto authResponse)
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
