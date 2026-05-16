using Microsoft.EntityFrameworkCore;
using ProjectHubAPI.Data;
using ProjectHubAPI.Interfaces;
using ProjectHubAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectHubAPI.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects.AsNoTracking().ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task AddAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
        }

        public async Task UpdateAsync(Project project)
        {
            _context.Projects.Update(project);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Project project)
        {
            _context.Projects.Remove(project);
            await Task.CompletedTask;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
 
