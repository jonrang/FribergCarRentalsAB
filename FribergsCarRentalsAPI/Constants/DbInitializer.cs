using FribergCarRentalsAPI.Data;
using Microsoft.AspNetCore.Identity;

namespace FribergCarRentalsAPI.Constants
{
    public static class DbInitializer
    {
        private const string AdminUserEmail = "admin@FribergCarRentals.se";

        public static async Task SeedData(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            var configuration = services.GetRequiredService<IConfiguration>();

            var adminPassword = configuration["SeedSettings:AdminPassword"];

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogError("Admin password not found in configuration/secrets. Cannot seed admin user.");
                return;
            }

            try
            {
                var userManager = services.GetRequiredService<UserManager<ApiUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                logger.LogInformation("Starting database seeding: Creating roles and initial admin user.");

                if (!await roleManager.RoleExistsAsync(ApiRoles.Administrator))
                {
                    await roleManager.CreateAsync(new IdentityRole(ApiRoles.Administrator));
                    logger.LogInformation("Created role: {Role}", ApiRoles.Administrator);
                }

                if (!await roleManager.RoleExistsAsync(ApiRoles.User))
                {
                    await roleManager.CreateAsync(new IdentityRole(ApiRoles.User));
                    logger.LogInformation("Created role: {Role}", ApiRoles.User);
                }

                var adminUser = await userManager.FindByEmailAsync(AdminUserEmail);

                if (adminUser == null)
                {
                    var user = new ApiUser
                    {
                        UserName = AdminUserEmail,
                        Email = AdminUserEmail,
                        FirstName = "System",
                        LastName = "Admin",
                        DriverLicenseNumber = "ABC12345",
                        DateOfBirth = new DateTime(1980, 1, 1),
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, adminPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, ApiRoles.Administrator);
                        logger.LogInformation("Successfully created and assigned role to Admin user: {Email}", AdminUserEmail);
                    }
                    else
                    {
                        logger.LogError("Admin user creation failed. Errors: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Admin user already exists. Skipping creation.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}
