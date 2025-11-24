using System.ComponentModel.DataAnnotations;

namespace FribergCarRentalsAPI.Dto.Users
{
    public class RegisterUserDto : LoginUserDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z0-9]{10,20}$", ErrorMessage = "Invalid license format.")]
        public string DriverLicenseNumber { get; set; }

        public string PhoneNumber { get; set; }
    }
}