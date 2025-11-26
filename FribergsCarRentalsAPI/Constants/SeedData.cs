using FribergCarRentalsAPI.Data;

namespace FribergCarRentalsAPI.Constants
{
    public class SeedData
    {
        public List<CarModel> CarModels { get; set; } = new List<CarModel>();
        public List<CarSeedDto> Cars { get; set; } = new List<CarSeedDto>();
        public List<TestUserSeedDto> TestUsers { get; set; } = new List<TestUserSeedDto>();
        public List<RentalSeedDto> Rentals { get; set; } = new List<RentalSeedDto>();
    }

    public class CarSeedDto
    {
        public string ModelName { get; set; }
        public string Make { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public decimal RatePerDay { get; set; }
        public int Mileage { get; set; }
    }

    public class TestUserSeedDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string DriverLicenseNumber { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class RentalSeedDto
    {
        public string UserEmail { get; set; }
        public string LicensePlate { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public RentalStatus Status { get; set; }
        public decimal RateAtTimeOfRental { get; set; }
    }
}
