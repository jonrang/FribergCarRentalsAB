using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FribergCarRentalsAB.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }  
        public string Make { get; set; }  
        public string Model { get; set; }   
        public int Year { get; set; }    
        public string LicensePlate { get; set; }
        public decimal RatePerDay { get; set; }    
        public int Mileage { get; set; }   
        public bool IsAvailable { get; set; } = true;
        public string ImageFileName { get; set; } = "default.png";
        [BindNever]
        [ValidateNever]
        public virtual ICollection<Rental> Rentals { get; set; }
    }
}

