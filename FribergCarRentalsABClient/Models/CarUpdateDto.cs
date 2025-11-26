using System.ComponentModel.DataAnnotations;

namespace FribergCarRentalsABClient.Models
{
    public class CarUpdateDto
    {
        [Required]
        public string LicensePlate { get; set; }
        public int Year { get; set; }
        [Required]
        public double RatePerDay { get; set; }
        public int Mileage { get; set; }
    }
}
