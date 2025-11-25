using System.ComponentModel.DataAnnotations;

namespace FribergCarRentalsAPI.Dto.Rentals
{
    public class CreateRentalDto
    {
        [Required]
        public int CarId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }
    }
}
