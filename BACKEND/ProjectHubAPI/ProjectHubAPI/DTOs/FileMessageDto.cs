using Microsoft.AspNetCore.Http;

namespace ProjectHubAPI.DTOs
{
    public class FileMessageDto
    {
        public int ReceiverId { get; set; }
        public string? Content { get; set; }
        public IFormFile File { get; set; }
    }
}
