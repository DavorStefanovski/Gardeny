using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gardeny.Data;
using Gardeny.Models;
using Gardeny.Viewmodels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Gardeny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PostsController(ApplicationDbContext context, IWebHostEnvironment enviroment)
        {
            _context = context;
            _environment = enviroment;
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            return await _context.Posts.ToListAsync();
        }

        // GET: api/Posts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        // PUT: api/Posts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, Post post)
        {
            if (id != post.PostId)
            {
                return BadRequest();
            }

            _context.Entry(post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

       

        // DELETE: api/Posts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
        // GET: api/Posts/UserPosts/{userId}
        [HttpGet("UserPosts/{userId}")]
        [Authorize] // Ensure the user is authenticated to access this endpoint
        public async Task<ActionResult<IEnumerable<PostFeed>>> GetPostsFromUser(int userId)
        {
            try
            {
                // Check if the userId is valid
                if (userId <= 0)
                {
                    return BadRequest("Invalid user ID.");
                }

                // Retrieve the logged-in user ID to ensure the user is authenticated
                var loggedInUserIdClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(loggedInUserIdClaimValue) || !int.TryParse(loggedInUserIdClaimValue, out int loggedInUserId))
                {
                    return Unauthorized("User ID claim is missing or invalid.");
                }

                
                // Check if the logged-in user follows the specified user
                var followsUser = await _context.Follows
                    .AnyAsync(f => f.FollowerId == loggedInUserId && f.FolloweeId == userId);

                if (!followsUser && (userId != loggedInUserId))
                {
                    return Forbid("You do not follow this user.");
                }

                // Query posts from the specified user sorted by date
                var userPosts = await _context.Posts
                    .AsNoTracking()
                    .Where(p => p.UserId == userId) // Posts from the specified user
                    .OrderByDescending(p => p.Date) // Order by date descending
                    .Include(p => p.Comments.OrderBy(c => c.Date)) // Include comments in ascending date order
                    .ThenInclude(c => c.User) // Include the user in each comment
                    .Include(p => p.Ratings) // Include ratings
                    .Include(p => p.Pictures)
                    .ToListAsync();

                // Create PostFeed view model list
                var postFeedList = new List<PostFeed>();

                // Populate PostFeed with required data
                foreach (var post in userPosts)
                {
                    var rating = await _context.Ratings
                         .Where(p => p.PostId == post.PostId && p.UserId == loggedInUserId)
                         .Select(p => p.Value)
                         .FirstOrDefaultAsync();

                    var postFeed = new PostFeed
                    {
                        Post = post, // Assign the entire Post object
                        
                        AverageRating = post.Ratings.Any() ? post.Ratings.Average(r => r.Value) : 0,// Calculate average rating for the post

                        Rating = rating
                    };

                    // Add to the list
                    postFeedList.Add(postFeed);
                }

                return Ok(postFeedList);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating post: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating post.");
            }
        }

        // GET: api/Posts/Feed
        [HttpGet("Feed")]
        [Authorize] // Ensure the user is authenticated to access this endpoint
        public async Task<ActionResult<IEnumerable<PostFeed>>> GetPostsFromFriends()
        {
            try
            {
                // Retrieve the logged-in user ID
                var userIdClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaimValue) || !int.TryParse(userIdClaimValue, out int userId))
                {
                    return Unauthorized("User ID claim is missing or invalid.");
                }

                // Query posts from friends sorted by date
                var friendIds = await _context.Follows
                    .Where(f => f.FollowerId == userId) // User is the follower
                    .Select(f => f.FolloweeId) // Select the IDs of the users being followed (friends)
                    .ToListAsync();

                friendIds.Add(userId);

                var postsFromFriends = await _context.Posts
                .AsNoTracking()
                .Where(p => friendIds.Contains(p.UserId)) // Posts from friends
                .OrderByDescending(p => p.Date) // Order by date descending
                .Include(p => p.Comments.OrderBy(c => c.Date)) // Include comments in ascending date order
                    .ThenInclude(c => c.User) // Include the user in each comment
                    .Include(p => p.Ratings) // Include ratings
                    .Include(p => p.Pictures)
                .Include(p => p.User)
                .ToListAsync();

                // Create PostDetails view model list
                var postDetailsList = new List<PostFeed>();

                // Populate PostDetails with required data
                foreach (var post in postsFromFriends)
                {
                     
                     var rating = await _context.Ratings
                         .Where(p =>  p.PostId == post.PostId && p.UserId.ToString() == userIdClaimValue )
                         .Select(p => p.Value)
                         .FirstOrDefaultAsync();
                     

                    var postDetails = new PostFeed
                    {
                        Post = post,
                        Rating = rating
                        
                    };

                    // Calculate average rating for the post
                    if (post.Ratings.Any())
                    {
                        postDetails.AverageRating = post.Ratings.Average(r => r.Value);
                    }
                    else
                    {
                        postDetails.AverageRating = 0; // Or set to a default value if no ratings exist
                    }
                    
                    // Add to the list
                    postDetailsList.Add(postDetails);
                }
                
                return Ok(postDetailsList);
                
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving posts from friends: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving posts from friends.");
            }
        }
        

        // POST: api/posts
        [HttpPost]
        [Authorize] // Requires authentication to access this endpoint
        public async Task<ActionResult> CreatePost( PostCreate model)
        {
            // Retrieve the logged-in user ID
            var userIdClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaimValue) || !int.TryParse(userIdClaimValue, out int userId))
            {
                return Unauthorized("User ID claim is missing or invalid.");
            }

            try
            {
                // Create a new post
                var post = new Post
                {
                    Date = DateTime.Now,
                    Text = model.Text,
                    UserId = userId,
                    Pictures = new List<Picture>(), // Initialize an empty list for pictures
                    Comments = new List<Comment>(), // Initialize an empty list for comments
                    Ratings = new List<Rating>()    // Initialize an empty list for ratings
                };

                // Save the post to the database
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                // Handle picture uploads
                if (!model.Files.IsNullOrEmpty())
                {
                    foreach (var file in model.Files)
                    {
                        // Ensure the file is not null and is valid
                        if (file == null || file.Length == 0)
                        {
                            continue;
                        }

                        // Generate a unique file name
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                        // Define the relative path within wwwroot where the file will be saved
                        var filePath = Path.Combine("uploads", fileName);

                        // Get the physical path to the wwwroot folder
                        var uploadFolder = Path.Combine(_environment.WebRootPath, filePath);

                        // Create the directory if it doesn't exist
                        Directory.CreateDirectory(Path.GetDirectoryName(uploadFolder));

                        // Save the file to wwwroot
                        using (var stream = new FileStream(uploadFolder, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Save picture information to database
                        var picture = new Picture
                        {
                            Url = "/" + filePath.Replace("\\", "/"), // Set the URL where the picture is saved (relative to wwwroot)
                            PostId = post.PostId, // Associate the picture with the newly created post
                            FileName = fileName // Save the file name
                        };

                        _context.Pictures.Add(picture);
                        await _context.SaveChangesAsync();

                        // Add the picture to the post's collection
                        post.Pictures.Add(picture);
                    }
                }
                return Ok("Post created successfully."); // Return success message
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating post: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating post.");
            }
        }
        // POST: api/Posts/Rating
        [HttpPost("Rating")]
        [Authorize] // Requires authentication to access this endpoint
        public async Task<ActionResult> CreateRating([FromBody] CreateRating cr)
        {
            if (cr == null)
            {
                return BadRequest("Invalid rating data.");
            }

            // Retrieve the logged-in user ID
            var userIdClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaimValue) || !int.TryParse(userIdClaimValue, out int userId))
            {
                return Unauthorized("User ID claim is missing or invalid.");
            }

            try
            {
                var existingRating = await _context.Ratings.Where(u => u.UserId.ToString() == userIdClaimValue && u.PostId == cr.PostId).FirstOrDefaultAsync();
                if (existingRating == null)
                {
                    // Create a new rating
                    var rating = new Rating
                    {
                        Value = cr.Value,
                        UserId = userId,
                        PostId = cr.PostId
                    };
                    _context.Ratings.Add(rating);
                }
                else
                {
                    existingRating.Value = cr.Value;
                    _context.Ratings.Update(existingRating);
                }
                await _context.SaveChangesAsync();
                return Ok("Rating created successfully."); // Return success message
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating Rating: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating Rating.");
            }
        }
        // POST: api/Posts/Comment
        [HttpPost("Comment")]
        [Authorize] // Requires authentication to access this endpoint
        public async Task<ActionResult> CreateComment([FromBody] CreateComment cc)
        {
            if (cc == null)
            {
                return BadRequest("Invalid comment data.");
            }

            // Retrieve the logged-in user ID
            var userIdClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaimValue) || !int.TryParse(userIdClaimValue, out int userId))
            {
                return Unauthorized("User ID claim is missing or invalid.");
            }

            try
            {
                // Create a new rating
                var comment = new Comment
                {
                    Text = cc.Text,
                    UserId = userId,
                    PostId = cc.PostId,
                    Date = DateTime.UtcNow
                };

                // Save the rating to the database
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                return Ok("Comment created successfully."); // Return success message
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating Comment: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating Comment.");
            }
        }
    }
}

