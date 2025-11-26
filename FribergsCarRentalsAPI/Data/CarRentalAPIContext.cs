using FribergCarRentalsAPI.Constants;

namespace FribergCarRentalsAPI.Data
{
    public class CarRentalAPIContext : IdentityDbContext<ApiUser>
    {
        public DbSet<Car> Cars { get; set; }
        public DbSet<CarModel> CarModels { get; set; }
        public DbSet<Rental> Rentals { get; set; }
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
                },
                new IdentityRole
                {
                    Name = ApiRoles.Suspended,
                    NormalizedName = ApiRoles.Suspended,
                    Id = "0b281efb-70c5-4751-814a-bb6c0baac936"
                }
            );
        }
    }
}
