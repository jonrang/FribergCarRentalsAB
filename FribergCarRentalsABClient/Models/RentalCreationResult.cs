using FribergCarRentalsABClient.Services.Base;

namespace FribergCarRentalsABClient.Models
{
    public class RentalCreationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public RentalViewDto? RentalDetails { get; set; }
    }
}
