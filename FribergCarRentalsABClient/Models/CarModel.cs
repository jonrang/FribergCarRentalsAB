namespace FribergCarRentalsABClient.Models
{
    public class CarModel
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; }
        public string LicensePlateSnippet { get; set; }
        public int Mileage { get; set; }

        // Model/Type Information (combined from CarModelDto & CarViewDto)
        public int CarModelId { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string BodyStyle { get; set; }
        public string ImageFileName { get; set; }

        // Rental/Rate Information
        public int Year { get; set; }
        public double RatePerDay { get; set; }
    }
}
