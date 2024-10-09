using System;

namespace Gardeny.Models
{
    public class Rating
    {
        public int RatingId { get; set; }
        public int Value { get; set; } // Rating value from 1 to 5

        // Foreign key to User
        public int UserId { get; set; }
        public User User { get; set; }

        // Foreign key to Post
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}
