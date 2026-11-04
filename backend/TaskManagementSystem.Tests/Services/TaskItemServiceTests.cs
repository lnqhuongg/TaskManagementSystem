using TaskManagementSystem.Commons.Exceptions;
using TaskManagementSystem.DTOs.TaskDTO;
using TaskManagementSystem.Enums;
using TaskManagementSystem.Models.Entity;
using TaskManagementSystem.Services.Impl;
using TaskManagementSystem.Tests.Helpers;
using Xunit;

namespace TaskManagementSystem.Tests.Services
{
    public class TaskItemServiceTests
    {
        [Fact]
        public async Task Create_ShouldAddNewTask_WhenValidRequest()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var user = new User
            {
                Username = "task_creator",
                Email = "creator@example.com",
                Password = "hashedpassword",
                Status = CommonStatus.Active,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var category = new Category
            {
                Name = "Công việc gấp",
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var taskService = new TaskItemServiceImpl(context);
            var request = new CreateTaskRequest
            {
                Title = "Hoàn thành báo cáo đồ án",
                Description = "Viết chương 12 và kiểm thử",
                Status = TaskItemStatus.Todo,
                Priority = TaskItemPriority.High,
                UserId = user.Id,
                CategoryId = category.Id,
                DueDate = DateTime.UtcNow.AddDays(3)
            };

            // Act
            var result = await taskService.Create(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Hoàn thành báo cáo đồ án", result.Title);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(category.Id, result.CategoryId);
            Assert.Equal("task_creator", result.UserName);
            Assert.Equal("Công việc gấp", result.CategoryName);
        }

        [Fact]
        public async Task Create_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var category = new Category
            {
                Name = "Danh mục hợp lệ",
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var taskService = new TaskItemServiceImpl(context);
            var request = new CreateTaskRequest
            {
                Title = "Task với User không tồn tại",
                UserId = 9999, // User ID giả định
                CategoryId = category.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => taskService.Create(request));
        }

        [Fact]
        public async Task GetAll_ShouldReturnTasks_WithIncludeUserAndCategory()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var user = new User
            {
                Username = "nguyen_van_a",
                Email = "a@test.com",
                Password = "pwd",
                Status = CommonStatus.Active,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var category = new Category
            {
                Name = "Dự án tốt nghiệp",
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            context.TaskItems.Add(new TaskItem
            {
                Title = "Lập trình backend",
                UserId = user.Id,
                CategoryId = category.Id,
                Status = TaskItemStatus.InProgress,
                Priority = TaskItemPriority.Medium,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                User = user,
                Category = category
            });
            await context.SaveChangesAsync();

            var taskService = new TaskItemServiceImpl(context);

            // Act: Gọi hàm GetAll đã tối ưu bằng Include
            var result = await taskService.GetAll(page: 1, pageSize: 10, keyword: "");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Total);
            var task = result.Data.First();
            Assert.Equal("Lập trình backend", task.Title);
            Assert.Equal("nguyen_van_a", task.UserName); // Chứng minh Include User hoạt động
            Assert.Equal("Dự án tốt nghiệp", task.CategoryName); // Chứng minh Include Category hoạt động
        }

        [Fact]
        public async Task Update_ShouldUpdateTaskProperties_Successfully()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var user = new User
            {
                Username = "user_task",
                Email = "user_task@test.com",
                Password = "pwd",
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var category = new Category
            {
                Name = "Danh mục 1",
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Tiêu đề cũ",
                Description = "Mô tả cũ",
                Status = TaskItemStatus.Todo,
                Priority = TaskItemPriority.Low,
                UserId = user.Id,
                CategoryId = category.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                User = user,
                Category = category
            };
            context.TaskItems.Add(task);
            await context.SaveChangesAsync();

            var taskService = new TaskItemServiceImpl(context);
            var updateRequest = new UpdateTaskRequest
            {
                Title = "Tiêu đề mới đã cập nhật",
                Description = "Mô tả mới",
                Status = TaskItemStatus.Completed,
                Priority = TaskItemPriority.High,
                UserId = user.Id,
                CategoryId = category.Id,
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var updatedResult = await taskService.Update(task.Id, updateRequest);

            // Assert
            Assert.Equal("Tiêu đề mới đã cập nhật", updatedResult.Title);
            Assert.Equal(TaskItemStatus.Completed, updatedResult.Status);
            Assert.Equal(TaskItemPriority.High, updatedResult.Priority);
        }
    }
}
