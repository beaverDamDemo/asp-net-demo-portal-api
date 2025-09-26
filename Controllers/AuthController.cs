using AspNetDemoPortalAPI.Data;
using AspNetDemoPortalAPI.Dto;
using AspNetDemoPortalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AspNetDemoPortalAPI.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly DemoPortalContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(DemoPortalContext context, IConfiguration config, ILogger<AuthController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email is already registered." });

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Username = dto.Username,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                username = user.Username
            });
        }


        [HttpPost("login")]
        public IActionResult Login(UserDto dto)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var token = GenerateJwtToken(user);
            return Ok(new { token, username = user.Username });
        }


        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"🔐 Incoming Authorization header: {authHeader}");

            var username = User.Identity?.Name;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation("Fetching profile for user: {Username}", username);
            _logger.LogInformation("\u001B[34m[PROFILE ACCESS]\u001B[0m User {Username} accessed their profile at {Time}", username, DateTime.UtcNow);

            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("⚠️ User.Identity.Name is missing or empty for authenticated request.");
                return BadRequest(new { error = "Username is missing from the authentication context." });
            }

            _logger.LogInformation("\u001B[34m[PROFILE ACCESS]\u001B[0m User '{Username}' accessed their profile", username);

            var response = new Dictionary<string, string>
            {
                ["username"] = username,
                ["email"] = email
            };

            return Ok(response);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("UserId", user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("debug-auth")]
        public IActionResult DebugAuth()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"🧪 DebugAuth header: {authHeader}");
            return Ok("Header logged.");
        }

    }
}
