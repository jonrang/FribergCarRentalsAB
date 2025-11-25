namespace FribergCarRentalsAPI.Data
{
    public class CarModel
    {
        public int CarModelId { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string BodyStyle { get; set; }
        public string ImageFileName { get; set; } = "default.png";
    }
}
