using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FribergCarRentalsAB.Models
{
    public class CreateRentalViewModel
    {
        [Required]
        [Display(Name = "Bil")]
        public int CarId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Datum")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Slut Datum")]
        public DateTime EndDate { get; set; }

        // Populated by controller
        public List<SelectListItem> Cars { get; set; } = new();
    }
}