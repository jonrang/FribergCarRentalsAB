using System.ComponentModel.DataAnnotations;

namespace FribergCarRentalsAB.Models
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
        public int UserId { get; set; }      // FK → ApplicationUser
        public int CarId { get; set; }      // FK → Car
        public DateTime StartDate { get; set; }     
        public DateTime EndDate { get; set; }    
        public DateTime? ActualReturnDate { get; set; }      // nullable until returned
        public decimal TotalCost { get; set; }      // computed (RatePerDay * days + fees)
        public RentalStatus Status { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Car Car { get; set; }

    }
}
