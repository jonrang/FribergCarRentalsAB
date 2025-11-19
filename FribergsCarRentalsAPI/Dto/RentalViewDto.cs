namespace FribergCarRentalsAPI.Dto
{
    public class RentalViewDto
    {
        public int RentalId { get; set; }
        public string CarDetails { get; set; } // e.g., "Toyota Camry (Model ID: 101)"
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Status { get; set; } // e.g., "Pending"
        public decimal TotalCost { get; set; }

        public string UserId { get; set; }
        public string UserEmail { get; set; }
    }
}
