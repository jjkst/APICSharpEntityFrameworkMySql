using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RukuServiceApi.Context;
using RukuServiceApi.Models;
using RukuServiceApi.Services;

namespace RukuServiceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context,
            IAuthService authService,
            ILogger<AuthController> logger
        )
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Uid))
                {
                    return BadRequest(new { message = "Email and UID are required" });
                }

                // Find user by email and UID (case-insensitive email comparison)
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == request.Email.ToLower() && u.Uid == request.Uid
                );

                if (user == null)
                {
                    _logger.LogWarning(
                        "Login attempt failed for email: {Email}, UID: {Uid}. User not found in database.",
                        request.Email,
                        request.Uid
                    );
                    return Unauthorized(
                        new
                        {
                            message = "Invalid credentials",
                            details = "User not found. Make sure the user exists and credentials are correct.",
                        }
                    );
                }

                // Generate JWT token
                var token = _authService.GenerateJwtToken(user);

                _logger.LogInformation("User {Email} logged in successfully", user.Email);

                return Ok(
                    new
                    {
                        token = token,
                        user = new
                        {
                            id = user.Id,
                            email = user.Email,
                            displayName = user.DisplayName,
                            role = user.Role.ToString(),
                            emailVerified = user.EmailVerified,
                        },
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
        {
            try
            {
                // Check if user already exists (case-insensitive email comparison)
                var existingUser = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == request.Email.ToLower() || u.Uid == request.Uid
                );

                if (existingUser != null)
                {
                    return Conflict(new { message = "User already exists" });
                }

                // Create new user
                var user = new User
                {
                    Email = request.Email,
                    Uid = request.Uid,
                    DisplayName = request.DisplayName ?? request.Email,
                    EmailVerified = request.EmailVerified,
                    Role = UserRole.Subscriber, // Default role
                    Provider = request.Provider,
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = _authService.GenerateJwtToken(user);

                _logger.LogInformation("New user registered: {Email}", user.Email);

                return Ok(
                    new
                    {
                        token = token,
                        user = new
                        {
                            id = user.Id,
                            email = user.Email,
                            displayName = user.DisplayName,
                            role = user.Role.ToString(),
                            emailVerified = user.EmailVerified,
                        },
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Uid { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Uid { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool EmailVerified { get; set; }
        public ProviderList Provider { get; set; }
    }
}
