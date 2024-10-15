using Microsoft.AspNetCore.Identity;

namespace MainAssignment.Model
{
    public class AppUser : IdentityUser
    {
        
        public string Password { get; set; }
        public string Role { get; set; }
        public string Avatar { get; set; }
        
    }
}
