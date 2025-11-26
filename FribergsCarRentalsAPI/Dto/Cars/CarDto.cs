using System.ComponentModel.DataAnnotations;

namespace FribergCarRentalsAPI.Dto.Cars
{
    public class CarDto
    {
        [Required]
        public int CarModelId { get; set; }
        [Required]
        public string Make { get; set; }
        public int Year { get; set; }
        [Required]
        public string LicensePlate { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal RatePerDay { get; set; }
        public int Mileage { get; set; }
    }
}
