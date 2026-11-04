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
                CategoryId = category.Id,
                DueDate = DateTime.UtcNow.AddDays(3)
            };

            // Act
            var result = await taskService.Create(request, user.Id, UserRole.User);

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
        public async Task Create_ShouldForceCurrentUserId_WhenRegularUserTriesToPassDifferentUserId()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var user1 = new User
            {
                Username = "user_one",
                Email = "user1@example.com",
                Password = "pwd",
                Status = CommonStatus.Active,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var user2 = new User
            {
                Username = "user_two",
                Email = "user2@example.com",
                Password = "pwd",
                Status = CommonStatus.Active,
                Role = UserRole.User,
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
            context.Users.AddRange(user1, user2);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var taskService = new TaskItemServiceImpl(context);
            var request = new CreateTaskRequest
            {
                Title = "Cố tình gán cho user2",
                UserId = user2.Id,
                CategoryId = category.Id
            };

            // Act: user1 là regular user gọi Create
            var result = await taskService.Create(request, user1.Id, UserRole.User);

            // Assert: task vẫn phải thuộc về user1
            Assert.Equal(user1.Id, result.UserId);
            Assert.Equal("user_one", result.UserName);
        }

        [Fact]
        public async Task Create_ShouldAssignToSpecifiedUserId_WhenAdminPassesUserId()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var admin = new User
            {
                Username = "admin_user",
                Email = "admin@example.com",
                Password = "pwd",
                Status = CommonStatus.Active,
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var staff = new User
            {
                Username = "staff_member",
                Email = "staff@example.com",
                Password = "pwd",
                Status = CommonStatus.Active,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var category = new Category
            {
                Name = "Giao việc nhân viên",
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.AddRange(admin, staff);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var taskService = new TaskItemServiceImpl(context);
            var request = new CreateTaskRequest
            {
                Title = "Việc được Admin giao",
                UserId = staff.Id,
                CategoryId = category.Id
            };

            // Act: Admin giao việc cho staff
            var result = await taskService.Create(request, admin.Id, UserRole.Admin);

            // Assert
            Assert.Equal(staff.Id, result.UserId);
            Assert.Equal("staff_member", result.UserName);
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
                UserId = 9999,
                CategoryId = category.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => taskService.Create(request, 9999, UserRole.Admin));
        }

        [Fact]
        public async Task GetAll_ShouldOnlyReturnUserTasks_WhenRoleIsUser()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var user1 = new User
            {
                Username = "user_one",
                Email = "user1@test.com",
                Password = "pwd",
                Status = CommonStatus.Active,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var user2 = new User
            {
                Username = "user_two",
                Email = "user2@test.com",
                Password = "pwd",
                Status = CommonStatus.Active,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var category = new Category
            {
                Name = "Danh mục chung",
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.AddRange(user1, user2);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            context.TaskItems.AddRange(
                new TaskItem
                {
                    Title = "Task của User 1",
                    UserId = user1.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TaskItem
                {
                    Title = "Task của User 2",
                    UserId = user2.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();

            var taskService = new TaskItemServiceImpl(context);

            // Act: user1 gọi GetAll
            var result = await taskService.GetAll(1, 10, "", user1.Id, UserRole.User);

            // Assert
            Assert.Equal(1, result.Total);
            Assert.Equal("Task của User 1", result.Data.First().Title);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllTasks_WhenRoleIsAdmin()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var admin = new User
            {
                Username = "admin_user",
                Email = "admin@test.com",
                Password = "pwd",
                Status = CommonStatus.Active,
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var user1 = new User
            {
                Username = "user_one",
                Email = "u1@test.com",
                Password = "pwd",
                Status = CommonStatus.Active,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var category = new Category
            {
                Name = "Danh mục chung",
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.AddRange(admin, user1);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            context.TaskItems.AddRange(
                new TaskItem
                {
                    Title = "Task 1",
                    UserId = admin.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new TaskItem
                {
                    Title = "Task 2",
                    UserId = user1.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();

            var taskService = new TaskItemServiceImpl(context);

            // Act: Admin gọi GetAll
            var result = await taskService.GetAll(1, 10, "", admin.Id, UserRole.Admin);

            // Assert
            Assert.Equal(2, result.Total);
        }

        [Fact]
        public async Task GetById_ShouldThrowForbiddenException_WhenRegularUserAccessesOtherUsersTask()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var user1 = new User { Username = "u1", Email = "u1@t.com", Password = "p", Status = CommonStatus.Active, Role = UserRole.User, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var user2 = new User { Username = "u2", Email = "u2@t.com", Password = "p", Status = CommonStatus.Active, Role = UserRole.User, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var category = new Category { Name = "Cat1", Status = CommonStatus.Active, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Users.AddRange(user1, user2);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var task = new TaskItem { Title = "Task của user 2", UserId = user2.Id, CategoryId = category.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.TaskItems.Add(task);
            await context.SaveChangesAsync();

            var taskService = new TaskItemServiceImpl(context);

            // Act & Assert: user1 cố xem task của user2
            await Assert.ThrowsAsync<ForbiddenException>(() => taskService.GetById(task.Id, user1.Id, UserRole.User));
        }

        [Fact]
        public async Task Update_ShouldUpdateTaskProperties_Successfully_WhenOwnerUpdates()
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
                CategoryId = category.Id,
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var updatedResult = await taskService.Update(task.Id, updateRequest, user.Id, UserRole.User);

            // Assert
            Assert.Equal("Tiêu đề mới đã cập nhật", updatedResult.Title);
            Assert.Equal(TaskItemStatus.Completed, updatedResult.Status);
            Assert.Equal(TaskItemPriority.High, updatedResult.Priority);
        }

        [Fact]
        public async Task Update_ShouldThrowForbiddenException_WhenRegularUserUpdatesOtherUsersTask()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var user1 = new User { Username = "u1", Email = "u1@t.com", Password = "p", Status = CommonStatus.Active, Role = UserRole.User, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var user2 = new User { Username = "u2", Email = "u2@t.com", Password = "p", Status = CommonStatus.Active, Role = UserRole.User, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var category = new Category { Name = "Cat1", Status = CommonStatus.Active, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Users.AddRange(user1, user2);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var task = new TaskItem { Title = "Task của user 2", UserId = user2.Id, CategoryId = category.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.TaskItems.Add(task);
            await context.SaveChangesAsync();

            var taskService = new TaskItemServiceImpl(context);
            var updateRequest = new UpdateTaskRequest { Title = "Hacked Title", CategoryId = category.Id };

            // Act & Assert: user1 cố tình sửa task của user2
            await Assert.ThrowsAsync<ForbiddenException>(() => taskService.Update(task.Id, updateRequest, user1.Id, UserRole.User));
        }

        [Fact]
        public async Task Update_ShouldAllowAdmin_ToUpdateAnyTask()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var admin = new User { Username = "admin", Email = "admin@t.com", Password = "p", Status = CommonStatus.Active, Role = UserRole.Admin, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var user = new User { Username = "u1", Email = "u1@t.com", Password = "p", Status = CommonStatus.Active, Role = UserRole.User, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var category = new Category { Name = "Cat1", Status = CommonStatus.Active, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Users.AddRange(admin, user);
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var task = new TaskItem { Title = "Task của user", UserId = user.Id, CategoryId = category.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.TaskItems.Add(task);
            await context.SaveChangesAsync();

            var taskService = new TaskItemServiceImpl(context);
            var updateRequest = new UpdateTaskRequest { Title = "Admin cập nhật task này", CategoryId = category.Id };

            // Act: Admin sửa task của user
            var updatedResult = await taskService.Update(task.Id, updateRequest, admin.Id, UserRole.Admin);

            // Assert
            Assert.Equal("Admin cập nhật task này", updatedResult.Title);
        }
    }
}
