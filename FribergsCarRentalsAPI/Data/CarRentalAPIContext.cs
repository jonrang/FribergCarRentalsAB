using FribergsCarRentalsAPI.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FribergsCarRentalsAPI.Data
{
    public class CarRentalAPIContext : IdentityDbContext<ApiUser>
    {
        public CarRentalAPIContext(DbContextOptions options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Name = ApiRoles.User,
                    NormalizedName = ApiRoles.User,
                    Id = "b224aef4-90cc-4178-951d-60c5863cef30"
                },
                new IdentityRole
                {
                    Name = ApiRoles.Administrator,
                    NormalizedName = ApiRoles.Administrator,
                    Id = "e27d61dc-c6ff-4f09-9777-d03cdba1c28f"
                }
            );

            //var hasher = new PasswordHasher<ApiUser>();

            builder.Entity<ApiUser>().HasData(
               new ApiUser
               {
                   Id = "1344d7a9-5f6e-4f0c-a8bf-c1111e8fe83e",
                   Email = "user@demoapi.com",
                   NormalizedEmail = "USER@DEMOAPI.COM",
                   UserName = "user@demoapi.com",
                   NormalizedUserName = "USER@DEMOAPI.COM",
                   FirstName = "System",
                   LastName = "User",
                   PasswordHash = "AQAAAAIAAYagAAAAEGIEgrETiBX+qqYrrTtfWhazQGxZUcWhbNhgsRhxy0Lqcqo9WRyABATkakMVgXjxMg==",
                   //PasswordHash = hasher.HashPassword(null, "Test1234!"),
                   EmailConfirmed = true,
                   ConcurrencyStamp = "118e6983-9de0-4501-80f2-cc6e0c3481bf",
                   SecurityStamp = "97ff03d6-50c7-4b01-8b6f-6e7481e5b67b"
               },
               new ApiUser
               {
                   Id = "111786f7-e6d4-40bc-94d2-bf42fed88f3d",
                   Email = "admin@demoapi.com",
                   NormalizedEmail = "ADMIN@DEMOAPI.COM",
                   UserName = "admin@demoapi.com",
                   NormalizedUserName = "ADMIN@DEMOAPI.COM",
                   FirstName = "System",
                   LastName = "Admin",
                   PasswordHash = "AQAAAAIAAYagAAAAELAVWEcptR2Z1GevgYjJq13BKzWjVhY2Kzpf9PToTDMHf2fFFNlnCXGbrDFvgeoe6w==",
                   //PasswordHash = hasher.HashPassword(null, "Test1234!"),
                   EmailConfirmed = true,
                   ConcurrencyStamp = "804c403d-949c-4b50-a650-20d230689ffd",
                   SecurityStamp = "4c6e020a-390d-491b-a283-dc8c827345a8"
               }
           );

            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "b224aef4-90cc-4178-951d-60c5863cef30",
                    UserId = "1344d7a9-5f6e-4f0c-a8bf-c1111e8fe83e"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "e27d61dc-c6ff-4f09-9777-d03cdba1c28f",
                    UserId = "111786f7-e6d4-40bc-94d2-bf42fed88f3d"
                }
            );
        }
    }
}
