using Gardeny.Models;
namespace Gardeny.Viewmodels
{
    public class UserDetails
    {
        public User User { get; set; }
        public bool Follow { get; set; }
        public int Following { get; set; }
        public int Followers { get; set; }
    }
}
