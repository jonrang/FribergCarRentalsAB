namespace FribergCarRentalsAPI.Dto
{
    public class RefreshTokenDto
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
