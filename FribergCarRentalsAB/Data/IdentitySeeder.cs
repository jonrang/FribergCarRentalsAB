using FribergCarRentalsAB.Models;
using Microsoft.AspNetCore.Identity;

namespace FribergCarRentalsAB.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
        {
            var roleMgr = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();

          
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole<int>(role));
            }

        }
    }
}
