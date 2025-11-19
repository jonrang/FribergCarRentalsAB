using FribergCarRentalsAPI.Constants;
using FribergCarRentalsAPI.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FribergCarRentalsAPI.Data.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<ApiUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly CarRentalAPIContext carRentalAPIContext;

        public AdminService(UserManager<ApiUser> userManager, RoleManager<IdentityRole> roleManager, CarRentalAPIContext carRentalAPIContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.carRentalAPIContext = carRentalAPIContext;
        }

        public async Task<IEnumerable<AdminUserViewDto>> GetAllUsersAsync()
        {
            var userRoleData = await carRentalAPIContext.Users
                .Select(u => new
                {
                        User = u,
                        Roles = (from userRole in carRentalAPIContext.UserRoles
                                join role in carRentalAPIContext.Roles on userRole.RoleId equals role.Id
                                where userRole.UserId == u.Id
                                select role.Name).ToList()
                })
                .ToListAsync();


            var userViews = userRoleData.Select(data => new AdminUserViewDto
            {
                Id = data.User.Id,
                Email = data.User.Email,
                FirstName = data.User.FirstName,
                LastName = data.User.LastName,
                Roles = data.Roles.ToArray()
            }).ToList();

            return userViews;
        }

        public Task<ApiUser?> GetUserByIdAsync(string userId)
        {
            return userManager.FindByIdAsync(userId);
        }

        public async Task<(bool Success, IDictionary<string, string[]> Errors)> UpdateUserDetailsAsync(string userId, UserDto updateDto)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new Dictionary<string, string[]> { { "User", new[] { "User not found." } } });
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;

            // Note: Do not allow changing email or password through a generic Update.
            // Use dedicated Identity methods for that (e.g., ChangeEmailAsync).

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return (false, MapIdentityErrors(result));
            }

            return (true, new Dictionary<string, string[]>());
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                // Indicate success even if user doesn't exist to prevent enumeration attack.
                return true;
            }

            var result = await userManager.DeleteAsync(user);

            return result.Succeeded;
        }

        public async Task<(bool Success, IDictionary<string, string[]> Errors)> ChangeUserRoleAsync(string userId, string newRole)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return (false, new Dictionary<string, string[]> { { "User", new[] { "User not found." } } });
            }

            if (newRole != ApiRoles.Administrator && newRole != ApiRoles.User)
            {
                return (false, new Dictionary<string, string[]> { { "Role", new[] { "Invalid role specified." } } });
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                return (false, MapIdentityErrors(removeResult));
            }

            var addResult = await userManager.AddToRoleAsync(user, newRole);

            if (!addResult.Succeeded)
            {
                return (false, MapIdentityErrors(addResult));
            }

            return (true, new Dictionary<string, string[]>());
        }

        private IDictionary<string, string[]> MapIdentityErrors(IdentityResult result)
        {
            return result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray()
                );
        }
    }
}
