using Microsoft.EntityFrameworkCore;
using ProjectHubAPI.Data;
using ProjectHubAPI.Interfaces;
using ProjectHubAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHubAPI.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync()
        {
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAsync(TaskItem task)
        {
            await _context.Tasks.AddAsync(task);
        }

        public async Task UpdateAsync(TaskItem task)
        {
            _context.Tasks.Update(task);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(TaskItem task)
        {
            _context.Tasks.Remove(task);
            await Task.CompletedTask;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
 
