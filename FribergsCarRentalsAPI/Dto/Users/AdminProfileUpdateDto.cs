namespace FribergCarRentalsAPI.Dto.Users
{
    public class AdminProfileUpdateDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string DriverLicenseNumber { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;
    }
}
