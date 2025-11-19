namespace FribergCarRentalsAPI.Dto
{
    public class AuthResponse
    {
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public string Email { get; set; }
    }
}
