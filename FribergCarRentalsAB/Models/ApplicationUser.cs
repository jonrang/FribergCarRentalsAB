using Microsoft.AspNetCore.Identity;

namespace FribergCarRentalsAB.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string DriverLicenseNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public virtual ICollection<Rental> Rentals { get; set; }

    }
}
