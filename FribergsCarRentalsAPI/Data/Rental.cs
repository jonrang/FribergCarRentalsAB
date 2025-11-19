using System.ComponentModel.DataAnnotations;

namespace FribergCarRentalsAPI.Data
{
    public enum RentalStatus
    {
        Pending,   // reserved but not yet picked up
        Active,    // currently out
        Completed, // returned
        Cancelled
    }

    public class Rental
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }      // FK → ApplicationUser
        public int CarId { get; set; }      // FK → Car
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int DaysBooked { get; set; } // EndDate - Startdate
        public DateTime? ActualReturnDate { get; set; }      // nullable until returned
        public decimal RateAtTimeOfRental { get; set; }
        public decimal Fees { get; set; }
        public decimal TotalCost { get; set; }      // computed (RateAtTimeOfRental * days + fees)
        public RentalStatus Status { get; set; }
        public virtual ApiUser User { get; set; }
        public virtual Car Car { get; set; }
    }
}
