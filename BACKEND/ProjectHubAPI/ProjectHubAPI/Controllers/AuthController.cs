using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectHubAPI.Data;
using ProjectHubAPI.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine($"[AUTH] Login attempt for {dto.Email}");
            
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            sw.Stop();
            Console.WriteLine($"[AUTH] DB Query took {sw.ElapsedMilliseconds}ms");

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            bool isPasswordValid = false;

            // Check if stored password is a BCrypt hash
            if (user.Password.StartsWith("$2") && user.Password.Length > 20)
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
            }
            else
            {
                // Fallback for plain-text migration
                isPasswordValid = (user.Password == dto.Password);

                if (isPasswordValid)
                {
                    // Auto-migrate user to BCrypt hash
                    user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[AUTH] Migrated user {user.Email} to BCrypt hash.");
                }
            }

            if (!isPasswordValid)
            {
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, User = new { user.Id, user.Name, user.Email, Role = user.Role?.Name } });
        }



        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var canConnect = await _context.Database.CanConnectAsync();
                sw.Stop();
                return Ok(new { Status = "Healthy", ConnectionTime = $"{sw.ElapsedMilliseconds}ms", CanConnect = canConnect });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Unhealthy", Error = ex.Message });
            }
        }

        private string GenerateJwtToken(Models.User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User") 
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

