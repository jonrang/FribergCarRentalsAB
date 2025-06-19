using System;
using System.ComponentModel.DataAnnotations;

namespace FribergCarRentalsAB.Models
{
    public class RentalInputModel
    {
        [Required] public int CarId { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}