using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectHubAPI.Data;
using ProjectHubAPI.Models;
using ProjectHubAPI.DTOs;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IO;
using System;
using Microsoft.AspNetCore.Hosting;

namespace ProjectHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MessageController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet("ping")]
        [AllowAnonymous]
        public IActionResult Ping() => Ok("Message Controller is Alive!");

        [HttpGet("{otherUserId}")]
        public async Task<IActionResult> GetConversation(int otherUserId)
        {
            try {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
                var userId = int.Parse(userIdStr);

                var messages = await _context.Messages
                    .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                                (m.SenderId == otherUserId && m.ReceiverId == userId))
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();

                return Ok(messages);
            } catch (Exception ex) {
                return StatusCode(500, $"DB Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            try {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
                var userId = int.Parse(userIdStr);

                var message = new Message
                {
                    SenderId = userId,
                    ReceiverId = dto.ReceiverId,
                    Content = dto.Content,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Messages.Add(message);
                
                // Add persistent notification for receiver
                var sender = await _context.Users.FindAsync(userId);
                var notification = new Notification
                {
                    UserId = dto.ReceiverId,
                    Title = "New Message",
                    Message = $"You received a new message from {sender?.Name ?? "a team member"}",
                    Type = "Chat",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    RelatedId = userId // Reference to sender
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();

                return Ok(message);
            } catch (Exception ex) {
                return StatusCode(500, $"Send Error: {ex.Message}");
            }
        }

        [HttpPost("file")]
        public async Task<IActionResult> SendMessageWithFile([FromForm] FileMessageDto dto)
        {
            try {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
                var userId = int.Parse(userIdStr);

                string? fileUrl = null;
                string? fileType = null;

                if (dto.File != null) {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "chat");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.File.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create)) {
                        await dto.File.CopyToAsync(fileStream);
                    }

                    fileUrl = "uploads/chat/" + uniqueFileName;
                    fileType = Path.GetExtension(dto.File.FileName).TrimStart('.').ToLower();
                }

                var message = new Message {
                    SenderId = userId,
                    ReceiverId = dto.ReceiverId,
                    Content = dto.Content ?? (dto.File != null ? "Shared a file" : ""),
                    FileUrl = fileUrl,
                    FileType = fileType,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Messages.Add(message);
                
                // Add persistent notification for receiver
                var sender = await _context.Users.FindAsync(userId);
                var notification = new Notification
                {
                    UserId = dto.ReceiverId,
                    Title = "New Media Message",
                    Message = $"{sender?.Name ?? "A team member"} shared a file with you",
                    Type = "Chat",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    RelatedId = userId // Reference to sender
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();

                return Ok(message);
            } catch (Exception ex) {
                return StatusCode(500, $"File Send Error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
                var userId = int.Parse(userIdStr);

                var message = await _context.Messages.FindAsync(id);
                if (message == null) return NotFound();

                if (message.SenderId != userId) return Forbid("You can only delete your own messages.");

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Message deleted successfully" });
            } catch (Exception ex) {
                return StatusCode(500, $"Delete Error: {ex.Message}");
            }
        }
    }
}
