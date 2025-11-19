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
        private readonly IAdminService adminService;

        public AdminController(IAdminService adminService)
        {
            this.adminService = adminService;
        }

        // GET /api/admin/users 
        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<AdminUserViewDto>>> GetUsers()
        {
            try
            {
                var users = await adminService.GetAllUsersAsync();

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
            var user = await adminService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { Message = $"User with ID {userId} not found." });
            }

            // You might want to map this to a specific AdminUserDto to avoid exposing 
            // sensitive fields like PasswordHash or SecurityStamp.
            return Ok(new { user.Id, user.Email, user.FirstName, user.LastName });
        }

        // PUT /api/admin/users/{id}
        [HttpPut("{userId}")]
        public async Task<IActionResult> EditUser([FromRoute] string userId, [FromBody] UserDto updateDto)
        {
            var (success, errors) = await adminService.UpdateUserDetailsAsync(userId, updateDto);

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
            return NoContent(); // 204 No Content typically for a successful PUT update
        }

        // DELETE /api/admin/users/{id}
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string userId)
        {
            bool success = await adminService.DeleteUserAsync(userId);

            if (!success)
            {
                return Problem("Failed to delete user due to a server error.", statusCode: 500);
            }
            return NoContent(); // 204 No Content typically for a successful DELETE
        }

        // POST /api/admin/users/{id}/role
        [HttpPost("{userId}/role")]
        public async Task<IActionResult> ChangeRole([FromRoute] string userId, [FromBody] string newRole)
        {
            var (success, errors) = await adminService.ChangeUserRoleAsync(userId, newRole);

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
