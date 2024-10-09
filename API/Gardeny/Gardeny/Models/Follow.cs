using System;

namespace Gardeny.Models
{
    public class Follow
    {
        public int FollowId { get; set; }

        // User who is following (Follower)
        public int FollowerId { get; set; }
        public User Follower { get; set; }

        // User who is being followed (Followee)
        public int FolloweeId { get; set; }
        public User Followee { get; set; }

     
    }
}
