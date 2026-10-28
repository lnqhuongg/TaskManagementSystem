using TaskManagementSystem.Commons.Exceptions;
using TaskManagementSystem.DTOs.UserDTO;
using TaskManagementSystem.Enums;
using TaskManagementSystem.Models.Entity;
using TaskManagementSystem.Services.Impl;
using TaskManagementSystem.Tests.Helpers;
using Xunit;

namespace TaskManagementSystem.Tests.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task Create_ShouldAddNewUser_AndHashPasswordWithBCrypt()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var userService = new UserServiceImpl(context);
            var request = new CreateUserRequest
            {
                Username = "phan_anh_dung",
                Email = "dung@example.com",
                Password = "Password@123456",
                ConfirmPassword = "Password@123456",
                Role = UserRole.User
            };

            // Act
            var result = await userService.Create(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("phan_anh_dung", result.Username);
            Assert.Equal("dung@example.com", result.Email);
            Assert.Equal(UserRole.User, result.Role);

            // Kiểm tra mật khẩu trong Database đã được hash (bắt đầu bằng $2)
            var userInDb = await context.Users.FindAsync(result.Id);
            Assert.NotNull(userInDb);
            Assert.StartsWith("$2", userInDb.Password);
            Assert.True(BCrypt.Net.BCrypt.Verify("Password@123456", userInDb.Password));
        }

        [Fact]
        public async Task Create_ShouldThrowConflictException_WhenUsernameAlreadyExists()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var userService = new UserServiceImpl(context);
            var request1 = new CreateUserRequest
            {
                Username = "duplicate_user",
                Email = "user1@example.com",
                Password = "Password@123456",
                ConfirmPassword = "Password@123456",
                Role = UserRole.User
            };
            await userService.Create(request1);

            var request2 = new CreateUserRequest
            {
                Username = "duplicate_user",
                Email = "user2@example.com",
                Password = "Password@123456",
                ConfirmPassword = "Password@123456",
                Role = UserRole.User
            };

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => userService.Create(request2));
        }

        [Fact]
        public async Task GetById_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var user = new User
            {
                Username = "find_me",
                Email = "findme@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("Password@123"),
                Role = UserRole.User,
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userService = new UserServiceImpl(context);

            // Act
            var result = await userService.GetById(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal("find_me", result.Username);
        }

        [Fact]
        public async Task GetAll_ShouldReturnPagedResult_WithCorrectPagination()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            context.Users.AddRange(
                new User { Username = "user1", Email = "u1@test.com", Password = "hash", Status = CommonStatus.Active, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { Username = "user2", Email = "u2@test.com", Password = "hash", Status = CommonStatus.Active, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new User { Username = "user3", Email = "u3@test.com", Password = "hash", Status = CommonStatus.Active, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var userService = new UserServiceImpl(context);

            // Act: Lấy trang 1 với pageSize = 2
            var result = await userService.GetAll(page: 1, pageSize: 2, keyword: "");

            // Assert
            Assert.Equal(3, result.Total);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(1, result.Page);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.TotalPages);
        }
    }
}

