using System.Text.Json;
namespace FribergCarRentalsAPI.Dto.Rentals
{
    public class RentalViewDto
    {
        public int RentalId { get; set; }
        public string CarDetails { get; set; } 
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Status { get; set; } 
        public decimal TotalCost { get; set; }

        public string UserId { get; set; }
        public string UserEmail { get; set; }
    }
}
