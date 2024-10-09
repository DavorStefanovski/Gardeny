using Gardeny.Models;

namespace Gardeny.Viewmodels
{
    public class RegisterUser
    {
        public User User { get; set; }
        public string Password { get; set; }
        public IFormFile ProfilePicture { get; set; } // New property for profile picture
    }
}
