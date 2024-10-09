using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
namespace Gardeny.Models
{
    

    public class User : IdentityUser<int>
    {
        public int Price { get; set; }
        public string? PictureUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Location { get; set; }

        // Navigation properties (nullable)
        public  ICollection<Comment>? Comments { get; set; }
        public  ICollection<Rating>? Ratings { get; set; }
        public  ICollection<User>? Followers { get; set; }
        public  ICollection<User>? Following { get; set; }
        public  ICollection<Post>? Posts { get; set; }
    }
}
