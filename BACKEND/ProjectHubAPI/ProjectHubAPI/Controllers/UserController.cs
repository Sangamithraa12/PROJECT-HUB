using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectHubAPI.Data;
using ProjectHubAPI.Models;

namespace ProjectHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new {
                    u.Id,
                    u.Name,
                    u.Email,
                    RoleName = u.Role != null ? u.Role.Name : "User"
                }).ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (!string.IsNullOrEmpty(user.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            var existing = await _context.Users.FindAsync(id);

            if (existing == null)
                return NotFound();

            existing.Name = user.Name;
            existing.Email = user.Email;
            
            if (!string.IsNullOrEmpty(user.Password) && user.Password != existing.Password)
            {
                existing.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }
            
            existing.RoleId = user.RoleId;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            var taskAssignments = await _context.TaskAssignments.Where(ta => ta.UserId == id).ToListAsync();
            if (taskAssignments.Any())
            {
                _context.TaskAssignments.RemoveRange(taskAssignments);
            }

            var comments = await _context.Comments.Where(c => c.UserId == id).ToListAsync();
            if (comments.Any())
            {
                _context.Comments.RemoveRange(comments);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User and related records deleted successfully." });
        }
    }
}

 
