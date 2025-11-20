using FribergCarRentalsAPI.Constants;
using FribergCarRentalsAPI.Data.Services;
using FribergCarRentalsAPI.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FribergCarRentalsAPI.Controllers
{
    [Authorize(Roles = ApiRoles.Administrator)]
    [Route("api/[controller]/users")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserService userService;

        public AdminController(IUserService userService)
        {
            this.userService = userService;
        }

        // GET /api/admin/users 
        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<AdminUserViewDto>>> GetUsers()
        {
            try
            {
                var users = await userService.GetAllUsersAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return Problem("An unexpected error occurred while retrieving the user list.", statusCode: 500);
            }
        }

        // GET /api/admin/users/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser([FromRoute] string userId)
        {
            var userView = await userService.GetUserByIdAsync(userId);

            if (userView == null)
            {
                return NotFound(new { Message = $"User with ID {userId} not found." });
            }

            return Ok(userView);
        }

        // PUT /api/admin/users/{id}
        [HttpPut("{userId}")]
        public async Task<IActionResult> EditUser([FromRoute] string userId, [FromBody] AdminProfileUpdateDto updateDto)
        {
            var (success, errors) = await userService.UpdateUserByAdminAsync(userId, updateDto);

            if (!success)
            {
                if (errors.ContainsKey("User")) return NotFound();

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
            return NoContent();
        }

        // DELETE /api/admin/users/{id}
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string userId)
        {
            bool success = await userService.DeleteUserAsync(userId);

            if (!success)
            {
                return Problem("Failed to delete user due to a server error.", statusCode: 500);
            }
            return NoContent();
        }

        // POST /api/admin/users/{id}/role
        [HttpPost("{userId}/role")]
        public async Task<IActionResult> ChangeRole([FromRoute] string userId, [FromBody] string newRole)
        {
            var (success, errors) = await userService.ChangeUserRoleAsync(userId, newRole);

            if (!success)
            {
                if (errors.ContainsKey("User")) return NotFound();
                if (errors.ContainsKey("Role")) return BadRequest(new { Message = errors["Role"].First() });

                return Problem(errors.First().Value.First(), statusCode: 500);
            }
            return NoContent();
        }
    }
}
