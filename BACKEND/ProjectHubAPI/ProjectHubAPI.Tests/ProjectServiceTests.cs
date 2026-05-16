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
    public class ProjectServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IWebHostEnvironment> _mockEnv;

        public ProjectServiceTests()
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
        public async Task GetAllProjects_ReturnsAllProjects()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Projects.Add(new Project { Id = 1, Name = "Test Project 1", Description = "Desc 1", FilesUrl = "", Status = "Active" });
            context.Projects.Add(new Project { Id = 2, Name = "Test Project 2", Description = "Desc 2", FilesUrl = "", Status = "Active" });
            await context.SaveChangesAsync();

            var service = new ProjectService(context, _mockEnv.Object, _mapper);

            // Act
            var result = await service.GetAllProjectsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetProjectById_ReturnsProject_WhenProjectExists()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Projects.Add(new Project { Id = 1, Name = "Test Project 1", Description = "Desc 1", FilesUrl = "", Status = "Active" });
            await context.SaveChangesAsync();

            var service = new ProjectService(context, _mockEnv.Object, _mapper);

            // Act
            var result = await service.GetProjectByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Project 1", result.Name);
        }

        [Fact]
        public async Task CreateProject_AddsProjectToDatabase()
        {
            // Arrange
            var context = GetDatabaseContext();
            var service = new ProjectService(context, _mockEnv.Object, _mapper);
            var createDto = new CreateProjectDto { Name = "New Project", Description = "New Desc" };

            // Act
            var result = await service.CreateProjectAsync(createDto);

            // Assert
            Assert.Equal(1, await context.Projects.CountAsync());
            Assert.Equal("New Project", result.Name);
        }
    }
}
 
