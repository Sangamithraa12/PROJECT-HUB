using Moq;
using ProjectHubAPI.Models;
using ProjectHubAPI.Services;
using ProjectHubAPI.Data;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Repositories;
using Mapster;
using MapsterMapper;
using ProjectHubAPI.Mapping;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ProjectHubAPI.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectHubAPI.Tests.Services
{
    public class ProjectServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<IHubContext<ChatHub>> _mockHubContext;
        private readonly Mock<IHubClients> _mockHubClients;
        private readonly Mock<IClientProxy> _mockClientProxy;

        public ProjectServiceTests()
        {
            var config = new TypeAdapterConfig();
            new MapsterRegister().Register(config);
            _mapper = new ServiceMapper(new Mock<IServiceProvider>().Object, config);
            _mockEnv = new Mock<IWebHostEnvironment>();
            
            _mockHubContext = new Mock<IHubContext<ChatHub>>();
            _mockHubClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();
            
            _mockHubContext.Setup(h => h.Clients).Returns(_mockHubClients.Object);
            _mockHubClients.Setup(c => c.All).Returns(_mockClientProxy.Object);
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

            var projectRepo = new ProjectRepository(context);
            var taskRepo = new TaskRepository(context);
            var service = new ProjectService(projectRepo, taskRepo, _mockEnv.Object, _mapper, _mockHubContext.Object);

            // Act
            var result = await service.GetAllProjectsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetProjectById_ReturnsProject_WhenProjectExists()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Projects.Add(new Project { Id = 1, Name = "Test Project 1", Description = "Desc 1", FilesUrl = "", Status = "Active" });
            await context.SaveChangesAsync();

            var projectRepo = new ProjectRepository(context);
            var taskRepo = new TaskRepository(context);
            var service = new ProjectService(projectRepo, taskRepo, _mockEnv.Object, _mapper, _mockHubContext.Object);

            // Act
            var result = await service.GetProjectByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Test Project 1", result.Data.Name);
        }

        [Fact]
        public async Task CreateProject_AddsProjectToDatabase()
        {
            // Arrange
            var context = GetDatabaseContext();
            var projectRepo = new ProjectRepository(context);
            var taskRepo = new TaskRepository(context);
            var service = new ProjectService(projectRepo, taskRepo, _mockEnv.Object, _mapper, _mockHubContext.Object);
            var createDto = new CreateProjectDto { Name = "New Project", Description = "New Desc" };

            // Act
            var result = await service.CreateProjectAsync(createDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, await context.Projects.CountAsync());
            Assert.Equal("New Project", result.Data.Name);
        }
    }
}
