namespace WagentjeApp.Models
{
    public class User
    {
        public string Username { get; set; }
        public int UserId { get; set; } // Of een andere unieke identificator
        public string Email { get; set; }
        public string Role { get; set; } // Bijvoorbeeld "Admin" of "User"
    }
}
