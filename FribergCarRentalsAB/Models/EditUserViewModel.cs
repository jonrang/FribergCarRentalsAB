namespace FribergCarRentalsAB.Models
{
    public class EditUserViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public List<string> AllRoles { get; set; } = new();
        public IList<string> SelectedRoles { get; set; } = new List<string>();

    }
}
