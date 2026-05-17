using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectHubAPI.Data;
using ProjectHubAPI.DTOs;
using ProjectHubAPI.Models;
using ProjectHubAPI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectHubAPI.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfig;

        public AuthServiceTests()
        {
            _mockConfig = new Mock<IConfiguration>();

            var mockJwtSection = new Mock<IConfigurationSection>();
            mockJwtSection.Setup(s => s["Key"]).Returns("super_secret_key_1234567890_long_key_for_testing_jwt");
            mockJwtSection.Setup(s => s["Issuer"]).Returns("ProjectHubAPI");
            mockJwtSection.Setup(s => s["Audience"]).Returns("ProjectHubUsers");

            _mockConfig.Setup(c => c.GetSection("Jwt")).Returns(mockJwtSection.Object);
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
        public async Task LoginAsync_ReturnsFail_WhenUserDoesNotExist()
        {
            // Arrange
            var context = GetDatabaseContext();
            var service = new AuthService(context, _mockConfig.Object);
            var loginDto = new LoginDto { Email = "nonexistent@test.com", Password = "Password123" };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid credentials", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ReturnsFail_WhenPasswordIsIncorrect()
        {
            // Arrange
            var context = GetDatabaseContext();
            var role = new Role { Id = 1, Name = "Employee" };
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123");
            context.Roles.Add(role);
            context.Users.Add(new User 
            { 
                Id = 1, 
                Email = "test@test.com", 
                Password = hashedPassword, 
                Name = "Test User",
                RoleId = 1
            });
            await context.SaveChangesAsync();

            var service = new AuthService(context, _mockConfig.Object);
            var loginDto = new LoginDto { Email = "test@test.com", Password = "WrongPassword123" };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid credentials", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ReturnsOk_WithValidBcryptPassword()
        {
            // Arrange
            var context = GetDatabaseContext();
            var role = new Role { Id = 1, Name = "Employee" };
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123");
            context.Roles.Add(role);
            context.Users.Add(new User 
            { 
                Id = 1, 
                Email = "test@test.com", 
                Password = hashedPassword, 
                Name = "Test User",
                RoleId = 1
            });
            await context.SaveChangesAsync();

            var service = new AuthService(context, _mockConfig.Object);
            var loginDto = new LoginDto { Email = "test@test.com", Password = "CorrectPassword123" };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Token);
            Assert.Equal("Test User", result.Data.User.Name);
            Assert.Equal("Employee", result.Data.User.Role);
        }

        [Fact]
        public async Task LoginAsync_ReturnsOk_AndMigratesPlaintextPasswordToBcrypt()
        {
            // Arrange
            var context = GetDatabaseContext();
            var role = new Role { Id = 1, Name = "Employee" };
            context.Roles.Add(role);
            context.Users.Add(new User 
            { 
                Id = 1, 
                Email = "test@test.com", 
                Password = "PlaintextPassword123",
                Name = "Test User",
                RoleId = 1
            });
            await context.SaveChangesAsync();

            var service = new AuthService(context, _mockConfig.Object);
            var loginDto = new LoginDto { Email = "test@test.com", Password = "PlaintextPassword123" };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            // Verify that the password in the DB was migrated and hashed with Bcrypt
            var updatedUser = await context.Users.FindAsync(1);
            Assert.NotNull(updatedUser);
            Assert.StartsWith("$2", updatedUser.Password);
            Assert.True(BCrypt.Net.BCrypt.Verify("PlaintextPassword123", updatedUser.Password));
        }
    }
}
