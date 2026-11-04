using Microsoft.Extensions.Configuration;
using TaskManagementSystem.Commons.Exceptions;
using TaskManagementSystem.DTOs.AuthDTO;
using TaskManagementSystem.Enums;
using TaskManagementSystem.Models.Entity;
using TaskManagementSystem.Services.Impl;
using TaskManagementSystem.Tests.Helpers;
using Xunit;

namespace TaskManagementSystem.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly IConfiguration _configuration;

        public AuthServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsASecretKeyForTestingJwtAuthenticationOnly1234567890" },
                { "Jwt:Issuer", "TaskManagementSystem" },
                { "Jwt:Audience", "TaskManagementSystemClient" },
                { "Jwt:DurationInMinutes", "60" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var rawPassword = "Admin@123456";
            var user = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword(rawPassword),
                Role = UserRole.Admin,
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var authService = new AuthenticationServiceImpl(context, _configuration);
            var request = new LoginRequest
            {
                Email = "admin@example.com",
                Password = rawPassword
            };

            // Act
            var response = await authService.Login(request);

            // Assert
            Assert.NotNull(response);
            Assert.False(string.IsNullOrEmpty(response.Token));
            Assert.Equal("admin@example.com", response.Email);
            Assert.Equal(UserRole.Admin, response.Role);
        }

        [Fact]
        public async Task Login_ShouldThrowUnauthorizedException_WhenPasswordIsIncorrect()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("CorrectPassword@123"),
                Role = UserRole.User,
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var authService = new AuthenticationServiceImpl(context, _configuration);
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword@123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => authService.Login(request));
        }

        [Fact]
        public async Task Login_ShouldThrowUnauthorizedException_WhenUserDoesNotExist()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var authService = new AuthenticationServiceImpl(context, _configuration);
            var request = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "AnyPassword@123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() => authService.Login(request));
        }

        [Fact]
        public async Task Login_ShouldThrowForbiddenException_WhenUserIsInactive()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var rawPassword = "User@123456";
            var user = new User
            {
                Username = "inactiveuser",
                Email = "inactive@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword(rawPassword),
                Role = UserRole.User,
                Status = CommonStatus.Inactive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var authService = new AuthenticationServiceImpl(context, _configuration);
            var request = new LoginRequest
            {
                Email = "inactive@example.com",
                Password = rawPassword
            };

            // Act & Assert
            await Assert.ThrowsAsync<ForbiddenException>(() => authService.Login(request));
        }
    }
}

