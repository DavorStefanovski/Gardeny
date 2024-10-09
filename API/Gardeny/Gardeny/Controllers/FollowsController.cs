using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gardeny.Data;
using Gardeny.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Gardeny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;


        public FollowsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Follows
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Follow>>> GetFollows()
        {
            return await _context.Follows.ToListAsync();
        }

        // GET: api/Follows/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Follow>> GetFollow(int id)
        {
            var follow = await _context.Follows.FindAsync(id);

            if (follow == null)
            {
                return NotFound();
            }

            return follow;
        }

        // PUT: api/Follows/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFollow(int id, Follow follow)
        {
            if (id != follow.FollowId)
            {
                return BadRequest();
            }

            _context.Entry(follow).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FollowExists(id))
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

        // POST: api/Follows
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Follow>> PostFollow(Follow follow)
        {
            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFollow", new { id = follow.FollowId }, follow);
        }

        // DELETE: api/Follows/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFollow(int id)
        {
            var follow = await _context.Follows.FindAsync(id);
            if (follow == null)
            {
                return NotFound();
            }

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FollowExists(int id)
        {
            return _context.Follows.Any(e => e.FollowId == id);
        }

        // GET: api/follows/follow/{followeeId}
        [HttpGet("follow/{followeeId}")]
        [Authorize] // Requires authentication to access this endpoint
        public async Task<IActionResult> FollowUser(int followeeId)
        {
            // Retrieve user ID from claims
            var userIdClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (string.IsNullOrEmpty(userIdClaimValue) || !int.TryParse(userIdClaimValue, out int userId))
            {
                return Unauthorized("User ID claim is missing or invalid.");
            }

            try
            {
                // Attempt to find the user by user ID
                var follower = await _userManager.FindByIdAsync(userId.ToString());

                if (follower == null)
                {
                    return Unauthorized("User not found.");
                }

                // Check if the follow relationship already exists
                var existingFollow = await _context.Follows
                    .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FolloweeId == followeeId);

                if (existingFollow != null)
                {
                    return BadRequest("You are already following this user.");
                }

                // Create a new follow relationship
                var follow = new Follow
                {
                    FollowerId = userId,
                    FolloweeId = followeeId
                };

                _context.Follows.Add(follow);
                await _context.SaveChangesAsync();

                return Ok("You are now following the user.");
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Exception in FollowUser: {ex}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
        }
        // GET: api/Follows/{userId}/followers
        [HttpGet("{userId}/followers")]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowers(int userId)
        {
            try
            {
                // Retrieve followers of the specified user
                var followers = await _context.Follows
                    .Where(f => f.FolloweeId == userId)
                    .Select(f => f.Follower) // Select the users who are followers
                    .ToListAsync();

                return Ok(followers);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving followers: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving followers.");
            }
        }
        // GET: api/Follows/{userId}/followers
        [HttpGet("{userId}/following")]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowing(int userId)
        {
            try
            {
                // Retrieve followers of the specified user
                var followers = await _context.Follows
                    .Where(f => f.FollowerId == userId)
                    .Select(f => f.Followee) // Select the users who are followers
                    .ToListAsync();

                return Ok(followers);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving followers: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving followers.");
            }
        }
    }
}

