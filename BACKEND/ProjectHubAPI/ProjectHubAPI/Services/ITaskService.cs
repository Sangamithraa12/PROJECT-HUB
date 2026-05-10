using ProjectHubAPI.DTOs;
using System.Collections.Generic;

namespace ProjectHubAPI.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetAllTasksAsync();
        Task<TaskDto?> GetTaskByIdAsync(int id);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto taskDto);
        Task<TaskDto?> UpdateTaskAsync(int id, CreateTaskDto taskDto);
        Task<bool> DeleteTaskAsync(int id);
        Task<bool> AssignTaskAsync(int taskId, int userId);
        Task<bool> UpdateStatusAsync(int taskId, string status, int currentUserId);
        Task<bool> SubmitProofAsync(int taskId, string proofUrl);
        Task<string?> UploadProofAsync(int taskId, string fileName, Stream fileStream);
        Task<string?> UploadFolderAsync(int taskId, List<(string fileName, Stream stream)> files);
        Task<CommentDto?> AddCommentAsync(int taskId, int userId, string content);
        Task<CommentDto?> AddCommentWithFileAsync(int taskId, int userId, string content, string fileName, Stream fileStream);
        Task<CommentDto?> UpdateCommentAsync(int commentId, string content);
        Task<bool> DeleteCommentAsync(int commentId);
    }
}
