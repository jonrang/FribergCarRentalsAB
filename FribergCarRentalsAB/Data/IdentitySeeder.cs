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

            // 1) Ensure roles exist
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole<int>(role));
            }

            // 2) (Optional) Seed a default admin
            //var adminEmail = "admin@yourapp.com";
            //var admin = await userMgr.FindByEmailAsync(adminEmail);
            //if (admin == null)
            //{
            //    admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
            //    await userMgr.CreateAsync(admin, "P@ssw0rd!");           // choose a secure default
            //    await userMgr.AddToRoleAsync(admin, "Admin");
            //}
        }
    }
}
