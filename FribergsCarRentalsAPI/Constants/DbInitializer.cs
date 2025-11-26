using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FribergCarRentalsAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            var environment = services.GetRequiredService<IHostEnvironment>();
            var userManager = services.GetRequiredService<UserManager<ApiUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<CarRentalAPIContext>();

            var adminPassword = configuration["SeedSettings:AdminPassword"];

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogError("Admin password not found in configuration/secrets. Cannot seed admin user.");
                return;
            }

            try
            {

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
                if (environment.IsDevelopment())
                {
                    logger.LogInformation("Environment is Development. Starting extensive test data seeding...");

                    if (!context.CarModels.Any() || !context.Cars.Any())
                    {
                        var seedFilePath = Path.Combine(environment.ContentRootPath, "Constants", "seeddata.json");
                        var json = await File.ReadAllTextAsync(seedFilePath);
                        var seedData = JsonSerializer.Deserialize<SeedData>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Converters = { new JsonStringEnumConverter() }
                        });

                        if (seedData == null)
                        {
                            logger.LogError("Failed to deserialize seeddata.json.");
                            return;
                        }

                        context.CarModels.AddRange(seedData.CarModels);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded {Count} Car Models.", seedData.CarModels.Count);

                        foreach (var userSeed in seedData.TestUsers)
                        {
                            if (await userManager.FindByEmailAsync(userSeed.Email) == null)
                            {
                                var user = new ApiUser
                                {
                                    UserName = userSeed.Email,
                                    Email = userSeed.Email,
                                    FirstName = userSeed.FirstName,
                                    LastName = userSeed.LastName,
                                    DateOfBirth = userSeed.DateOfBirth,
                                    DriverLicenseNumber = userSeed.DriverLicenseNumber,
                                    PhoneNumber = userSeed.PhoneNumber,
                                    EmailConfirmed = true
                                };
                                var result = await userManager.CreateAsync(user, userSeed.Password);
                                if (result.Succeeded)
                                {
                                    await userManager.AddToRoleAsync(user, ApiRoles.User);
                                }
                            }
                        }
                        logger.LogInformation("Seeded {Count} Test Users.", seedData.TestUsers.Count);


                        var carModelsMap = context.CarModels.ToDictionary(m => m.Name);
                        foreach (var carSeed in seedData.Cars)
                        {
                            if (context.Cars.Any(c => c.LicensePlate == carSeed.LicensePlate)) continue;

                            if (carModelsMap.TryGetValue(carSeed.ModelName, out var model))
                            {
                                context.Cars.Add(new Car
                                {
                                    CarModelId = model.CarModelId,
                                    Make = carSeed.Make,
                                    Year = carSeed.Year,
                                    LicensePlate = carSeed.LicensePlate,
                                    RatePerDay = carSeed.RatePerDay,
                                    Mileage = carSeed.Mileage,
                                    IsAvailable = true
                                });
                            }
                        }
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded {Count} Cars.", seedData.Cars.Count);


                        var testUser = await userManager.FindByEmailAsync(seedData.Rentals.First().UserEmail);
                        var rentalCar = await context.Cars.FirstOrDefaultAsync(c => c.LicensePlate == seedData.Rentals.First().LicensePlate);

                        if (testUser != null && rentalCar != null)
                        {
                            var rentalSeed = seedData.Rentals.First();
                            var days = (rentalSeed.EndDate.ToDateTime(TimeOnly.MinValue) - rentalSeed.StartDate.ToDateTime(TimeOnly.MinValue)).Days;

                            context.Rentals.Add(new Rental
                            {
                                UserId = testUser.Id,
                                CarId = rentalCar.Id,
                                StartDate = rentalSeed.StartDate,
                                EndDate = rentalSeed.EndDate,
                                DaysBooked = days,
                                RateAtTimeOfRental = rentalSeed.RateAtTimeOfRental,
                                TotalCost = rentalSeed.RateAtTimeOfRental * days,
                                Status = rentalSeed.Status,
                            });
                            await context.SaveChangesAsync();
                            logger.LogInformation("Seeded 1 Test Rental.");
                        }
                    }
                    else
                    {
                        logger.LogInformation("Test data already exists. Skipping data seeding.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}
