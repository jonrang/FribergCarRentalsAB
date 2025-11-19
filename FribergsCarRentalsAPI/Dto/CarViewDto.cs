namespace FribergCarRentalsAPI.Dto
{
    public class CarViewDto
    {
        public int CarId { get; set; }
        public string LicensePlateSnippet { get; set; } // Last 4 chars for internal use/pickup
        public int Year { get; set; }
        public decimal RatePerDay { get; set; }
        public string ModelName { get; set; }
        public string Manufacturer { get; set; }
        public string BodyStyle { get; set; }
        public string ImageFileName { get; set; }
    }
}
