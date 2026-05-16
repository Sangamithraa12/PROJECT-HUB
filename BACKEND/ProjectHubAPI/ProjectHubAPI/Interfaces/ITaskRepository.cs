using ProjectHubAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectHubAPI.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task<IEnumerable<TaskItem>> GetByProjectIdAsync(int projectId);
        Task<TaskItem?> GetByIdAsync(int id);
        Task AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(TaskItem task);
        Task<bool> SaveChangesAsync();
    }
}
 
