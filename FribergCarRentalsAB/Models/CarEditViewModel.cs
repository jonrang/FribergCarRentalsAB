using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FribergCarRentalsAB.Models
{
    public class CarEditViewModel : CarCreateViewModel
    {
        public int Id { get; set; }
        public string? CurrentImageFileName { get; set; }
        [Display(Name = "Tillgänglig")]
        public bool IsAvailable { get; set; }
    }

}
