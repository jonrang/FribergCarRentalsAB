namespace FribergCarRentalsAPI.Dto.Cars
{
    public class CarModelDto
    {
        [Required]
        public int CarModelId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Manufacturer { get; set; }
        [Required]
        public string BodyStyle { get; set; }
        public string ImageFileName { get; set; }
    }
}
