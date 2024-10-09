using System;
using System.Collections.Generic;
namespace Gardeny.Models
    {
        public class Post
        {
            public int PostId { get; set; }
            public DateTime Date { get; set; }
            public string? Text { get; set; }

            // Foreign key to User
            public int UserId { get; set; }
            public User User { get; set; }

            // Navigation properties
            public ICollection<Picture>? Pictures { get; set; }
            public ICollection<Comment>? Comments { get; set; }
            public ICollection<Rating>? Ratings { get; set; }
        }
}

