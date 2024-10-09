using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gardeny.Data;
using Gardeny.Models;
using Microsoft.AspNetCore.Identity;
using Gardeny.Viewmodels;
using Gardeny.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Gardeny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly TokenService _tokenService;
        private readonly IWebHostEnvironment _environment;
        public UsersController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, TokenService tokenService, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _environment = environment;
        }

        // GET: api/Users/
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> SearchUsers(string searchString = "")
        {
            if (string.IsNullOrEmpty(searchString))
            {
                return Ok(new List<User>());
            }
            // Query users based on first name or last name containing the search string
            IQueryable<User> query = _context.Users
                .Where(u => EF.Functions.Like(u.FirstName, $"{searchString}%") || EF.Functions.Like(u.LastName, $"{searchString}%"))
                .Take(3);

            try
            {
                var users = await query.ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error searching users: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error searching users.");
            }
        }







        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        // api/Users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register( RegisterUser u)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = u.User.Email,
                    Email = u.User.Email,
                    FirstName = u.User.FirstName,
                    LastName = u.User.LastName,
                    DateOfBirth = u.User.DateOfBirth,
                    Location = u.User.Location,
                    Price = u.User.Price 
                };

                // Handle profile picture upload
                if (u.ProfilePicture != null && u.ProfilePicture.Length > 0)
                {
                    // Generate a unique file name to prevent overwriting existing files
                    var fileName = $"{Guid.NewGuid().ToString()}_{Path.GetFileName(u.ProfilePicture.FileName)}";
                    var filePath = Path.Combine(_environment.WebRootPath, "profile_pictures", fileName);

                    // Save the file to disk
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await u.ProfilePicture.CopyToAsync(stream);
                    }

                    // Assign the profile picture URL to the user object
                    user.PictureUrl = $"/profile_pictures/{fileName}";
                }

                var result = await _userManager.CreateAsync(user, u.Password);

                if (result.Succeeded)
                {
                    return Ok("User created successfully.");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return BadRequest(ModelState);
        }

        // api/Users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUser model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var token = _tokenService.GenerateToken(user);

                    return Ok(new { token });
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return BadRequest(ModelState);
            }

            return BadRequest(ModelState);
        }

        // api/Users/id
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetails>> GetUserDetails(int id)
        {
            // Retrieve user ID from claims
            var userIdClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaimValue) || !int.TryParse(userIdClaimValue, out int UserId))
            {
                return Unauthorized("Invalid user ID claim.");
            }

            try
            {
                // Retrieve the user from UserManager
                var user = await _userManager.FindByIdAsync(id.ToString());

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Check if the current user follows this user
                var isFollowedByCurrentUser = await _context.Follows
                    .AnyAsync(f => f.FollowerId == UserId && f.FolloweeId == id);

                // Count the number of users the user follows
                var followingCount = await _context.Follows
                    .CountAsync(f => f.FollowerId == id);

                // Count the number of followers for the user
                var followersCount = await _context.Follows
                    .CountAsync(f => f.FolloweeId == id);

                // Map the user details and follow information to UserDetails view model
                var userDetails = new UserDetails
                {
                    User = user,
                    Follow = isFollowedByCurrentUser,
                    Following = followingCount,
                    Followers = followersCount

                };

                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving user details: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving user details.");
            }

        }
        // api/Users/current
        [HttpGet("current")]
        [Authorize]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            // Retrieve user ID from claims
            var userIdClaimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaimValue) || !int.TryParse(userIdClaimValue, out int UserId))
            {
                return Unauthorized("Invalid user ID claim.");
            }

            try
            {
                // Retrieve the user from UserManager
                var user = await _userManager.FindByIdAsync(UserId.ToString());

                if (user == null)
                {
                    return NotFound("User not found.");
                }
                
                return Ok(user);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving current User: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving current User.");
            }

        }



    }
}
