using System;

namespace Gardeny.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }

        // Foreign key to User
        public int UserId { get; set; }
        public User User { get; set; }

        // Foreign key to Post
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}
