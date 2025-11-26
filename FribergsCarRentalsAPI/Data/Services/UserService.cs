using FribergCarRentalsAPI.Constants;
using FribergCarRentalsAPI.Dto.Users;

namespace FribergCarRentalsAPI.Data.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApiUser> userManager;
        private readonly CarRentalAPIContext carRentalAPIContext;

        public UserService(UserManager<ApiUser> userManager, CarRentalAPIContext carRentalAPIContext)
        {
            this.userManager = userManager;
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
                DateOfBirth = data.User.DateOfBirth,
                DriverLicenseNumber = data.User.DriverLicenseNumber,
                PhoneNumber = data.User.PhoneNumber,
                Roles = data.Roles.ToArray()
            }).ToList();

            return userViews;
        }

        public async Task<AdminUserViewDto> GetUserByIdAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var roles = await userManager.GetRolesAsync(user);

            return new AdminUserViewDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                DriverLicenseNumber = user.DriverLicenseNumber,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToArray()
            };
        }

        public async Task<(bool Success, IDictionary<string, string[]> Errors)> UpdateUserByAdminAsync(string userId, AdminProfileUpdateDto updateDto)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new Dictionary<string, string[]> { { "User", new[] { "User not found." } } });
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.DateOfBirth = updateDto.DateOfBirth;
            user.DriverLicenseNumber = updateDto.DriverLicenseNumber;
            user.PhoneNumber = updateDto.PhoneNumber;

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return (false, MapIdentityErrors(result));
            }

            return (true, new Dictionary<string, string[]>());
        }

        public async Task<(bool Success, IDictionary<string, string[]> Errors)> UpdateUserProfileAsync(string userId, CustomerProfileUpdateDto updateDto)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new Dictionary<string, string[]> { { "User", new[] { "User not found." } } });
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.PhoneNumber = updateDto.PhoneNumber;

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

        public async Task<(bool Success, IDictionary<string, string[]> Errors)> ChangeUserPasswordAsync(string userId, ChangePasswordDto changeDto)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (true, new Dictionary<string, string[]>());
            }

            var result = await userManager.ChangePasswordAsync(user, changeDto.CurrentPassword, changeDto.NewPassword);

            if (!result.Succeeded)
            {
                return (false, MapIdentityErrors(result));
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
