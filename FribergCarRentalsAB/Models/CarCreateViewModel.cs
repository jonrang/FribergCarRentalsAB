using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

public class CarCreateViewModel
{
    [Required, StringLength(50)]
    public string Make { get; set; }

    [Required, StringLength(50)]
    public string Model { get; set; }

    [Range(1900, 2100)]
    public int Year { get; set; }

    [Required, RegularExpression(@"^[A-Z0-9\-]+$",
        ErrorMessage = "uppercase, digits, hyphens, nothing else.")]
    [Display(Name = "License Plate")]
    public string LicensePlate { get; set; }

    [Range(0, 1000000)]
    [Display(Name = "Mileage (km)")]
    public int Mileage { get; set; }

    [Range(0, 10000)]
    [Display(Name = "Rate per Day")]
    public decimal RatePerDay { get; set; }

    [Display(Name = "Upload Image")]
    public IFormFile? ImageUpload { get; set; }
    public SelectList? ExistingImages { get; set; }
    public string? SelectedImageFileName { get; set; }
}