using Moq;
using ProjectHubAPI.Models;
using ProjectHubAPI.Services;
using ProjectHubAPI.Data;
using ProjectHubAPI.DTOs;
using Mapster;
using MapsterMapper;
using ProjectHubAPI.Mapping;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace ProjectHubAPI.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IWebHostEnvironment> _mockEnv;

        public TaskServiceTests()
        {
            var config = new TypeAdapterConfig();
            new MapsterRegister().Register(config);
            _mapper = new ServiceMapper(new Mock<IServiceProvider>().Object, config);
            _mockEnv = new Mock<IWebHostEnvironment>();
        }

        private AppDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetAllTasks_ReturnsAllTasks()
        {
            // Arrange
            var context = GetDatabaseContext();
            var project = new Project { Id = 1, Name = "Test Project", FilesUrl = "", Status = "Active" };
            var user = new User { Id = 1, Name = "Test User" };
            context.Projects.Add(project);
            context.Users.Add(user);
            context.Tasks.Add(new TaskItem { Id = 1, Title = "Task 1", ProjectId = 1, AssignedTo = 1 });
            context.Tasks.Add(new TaskItem { Id = 2, Title = "Task 2", ProjectId = 1, AssignedTo = 1 });
            await context.SaveChangesAsync();

            var service = new TaskService(context, _mockEnv.Object, _mapper);

            // Act
            var result = await service.GetAllTasksAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal("Test Project", t.ProjectName));
        }

        [Fact]
        public async Task UpdateStatus_ReturnsTrue_AndAddsNotification()
        {
            // Arrange
            var context = GetDatabaseContext();
            var task = new TaskItem { Id = 1, Title = "Task 1", Status = "Pending", AssignedTo = 1 };
            context.Tasks.Add(task);
            context.Users.Add(new User { Id = 1, Name = "User 1" });
            var managerRole = new Role { Id = 1, Name = "Manager" };
            context.Roles.Add(managerRole);
            context.Users.Add(new User { Id = 2, Name = "Manager 1", RoleId = 1 });
            await context.SaveChangesAsync();

            var service = new TaskService(context, _mockEnv.Object, _mapper);

            // Act
            var result = await service.UpdateStatusAsync(1, "Completed");

            // Assert
            Assert.True(result);
            Assert.Equal("Completed", task.Status);
            Assert.True(await context.Notifications.AnyAsync(n => n.UserId == 1)); 
            Assert.True(await context.Notifications.AnyAsync(n => n.UserId == 2)); 
        }
    }
}
 
