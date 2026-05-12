using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Services;
using System.Linq;

namespace ProjectHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            return Ok(await _taskService.GetAllTasksAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateTask(CreateTaskDto taskDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _taskService.CreateTaskAsync(taskDto);
            return CreatedAtAction(nameof(GetTask), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateTask(int id, CreateTaskDto taskDto)
        {
            var result = await _taskService.UpdateTaskAsync(id, taskDto);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var success = await _taskService.DeleteTaskAsync(id);
            if (!success) return NotFound();

            return Ok();
        }

        [HttpPost("{id}/assign")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AssignTask(int id, [FromBody] int userId)
        {
            var success = await _taskService.AssignTaskAsync(id, userId);
            if (!success) return NotFound("Task or User not found");

            return Ok("Task Assigned Successfully");
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var success = await _taskService.UpdateStatusAsync(id, status, userId);
            if (!success) return NotFound();

            return Ok();
        }

        [HttpPost("{id}/proof")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> SubmitProof(int id, [FromBody] string proofUrl)
        {
            var success = await _taskService.SubmitProofAsync(id, proofUrl);
            if (!success) return NotFound();

            return Ok();
        }

        [HttpPost("{id}/upload")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> UploadProof(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is empty");

            using (var stream = file.OpenReadStream())
            {
                var fileUrl = await _taskService.UploadProofAsync(id, file.FileName, stream);
                if (string.IsNullOrEmpty(fileUrl)) return NotFound("Task not found");

                return Ok(new { url = fileUrl });
            }
        }

        [HttpPost("{id}/upload-folder")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> UploadFolder(int id, List<IFormFile> files)
        {
            if (files == null || files.Count == 0) return BadRequest("No files uploaded");

            var fileData = new List<(string fileName, Stream stream)>();
            foreach (var file in files)
            {
                fileData.Add((file.FileName, file.OpenReadStream()));
            }

            var folderUrl = await _taskService.UploadFolderAsync(id, fileData);
            if (string.IsNullOrEmpty(folderUrl)) return NotFound("Task not found");

            return Ok(new { url = folderUrl });
        }

        [HttpPost("{id}/comment")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> AddComment(int id, [FromBody] string content)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var result = await _taskService.AddCommentAsync(id, userId, content);
            if (result == null) return NotFound("Task not found");

            return Ok(result);
        }

        [HttpPut("comment/{id}")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] string content)
        {
            var result = await _taskService.UpdateCommentAsync(id, content);
            if (result == null) return NotFound("Comment not found");

            return Ok(result);
        }

        [HttpPost("{taskId}/comment/file")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> AddCommentWithFile(int taskId, [FromForm] string content, IFormFile file)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            var userId = int.Parse(userIdStr);

            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            using (var stream = file.OpenReadStream())
            {
                var result = await _taskService.AddCommentWithFileAsync(taskId, userId, content, file.FileName, stream);
                if (result == null) return NotFound("Task not found");
                return Ok(result);
            }
        }

        [HttpDelete("comment/{id}")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var success = await _taskService.DeleteCommentAsync(id);
            if (!success) return NotFound("Comment not found");

            return Ok();
        }
    }
}
