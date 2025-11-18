namespace FribergsCarRentalsAPI.Dto
{
    public class AuthResponse
    {
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Email { get; set; }
    }
}
