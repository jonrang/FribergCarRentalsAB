using System.ComponentModel.DataAnnotations;

namespace FribergCarRentalsAPI.Dto
{
    public class CustomerProfileUpdateDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
