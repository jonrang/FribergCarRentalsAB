// Models/RentalIndexViewModel.cs
namespace FribergCarRentalsAB.Models
{
    public class RentalIndexViewModel
    {
        public IEnumerable<Rental> Current { get; set; } = new List<Rental>();
        public IEnumerable<Rental> History { get; set; } = new List<Rental>();
    }
}