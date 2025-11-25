using System.ComponentModel.DataAnnotations;

namespace FribergCarRentalsABClient.Models
{
    public class CarCreateDto
    {
        [Required]
        public int CarModelId { get; set; }
        public string Make { get; set; }
        public int Year { get; set; }
        [Required]
        public string LicensePlate { get; set; }
        [Required]
        public double RatePerDay { get; set; }
        public int Mileage { get; set; }
    }
}
