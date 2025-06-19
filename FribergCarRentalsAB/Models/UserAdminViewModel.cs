namespace FribergCarRentalsAB.Models
{
    public class UserAdminViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public IList<string> Roles { get; set; } = new List<string>();

    }
}
