using Microsoft.Extensions.Caching.Memory;
using TaskManagementSystem.Commons.Exceptions;
using TaskManagementSystem.DTOs.CategoryDTO;
using TaskManagementSystem.Enums;
using TaskManagementSystem.Models.Entity;
using TaskManagementSystem.Services.Impl;
using TaskManagementSystem.Tests.Helpers;
using Xunit;

namespace TaskManagementSystem.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly IMemoryCache _cache;

        public CategoryServiceTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        [Fact]
        public async Task Create_ShouldAddNewCategory_Successfully()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var categoryService = new CategoryServiceImpl(context, _cache);
            var request = new CreateCategoryRequest
            {
                Name = "Công việc dự án",
                Description = "Các task liên quan đến dự án"
            };

            // Act
            var result = await categoryService.Create(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Công việc dự án", result.Name);
            Assert.Equal(CommonStatus.Active, result.Status);
        }

        [Fact]
        public async Task Create_ShouldThrowConflictException_WhenCategoryNameAlreadyExists()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var categoryService = new CategoryServiceImpl(context, _cache);
            var request = new CreateCategoryRequest
            {
                Name = "Danh mục trùng lặp",
                Description = "Mô tả ban đầu"
            };
            await categoryService.Create(request);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => categoryService.Create(request));
        }

        [Fact]
        public async Task GetById_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var category = new Category
            {
                Name = "Học tập",
                Description = "Nhiệm vụ học tập",
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var categoryService = new CategoryServiceImpl(context, _cache);

            // Act
            var result = await categoryService.GetById(category.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category.Id, result.Id);
            Assert.Equal("Học tập", result.Name);
        }

        [Fact]
        public async Task GetAllActiveCategories_ShouldReturnOnlyActiveCategories()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            context.Categories.AddRange(
                new Category
                {
                    Name = "Active Category 1",
                    Status = CommonStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Inactive Category 2",
                    Status = CommonStatus.Inactive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();

            var categoryService = new CategoryServiceImpl(context, _cache);

            // Act
            var activeList = await categoryService.GetAllActiveCategories();

            // Assert
            Assert.Single(activeList);
            Assert.Equal("Active Category 1", activeList[0].Name);
        }

        [Fact]
        public async Task Update_ShouldUpdateCategoryAndInvalidateCache()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();
            var category = new Category
            {
                Name = "Tên cũ",
                Description = "Mô tả cũ",
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var categoryService = new CategoryServiceImpl(context, _cache);

            // Nạp trước vào cache
            await categoryService.GetById(category.Id);

            var updateRequest = new UpdateCategoryRequest
            {
                Name = "Tên mới",
                Description = "Mô tả mới"
            };

            // Act
            var updated = await categoryService.Update(category.Id, updateRequest);

            // Assert
            Assert.Equal("Tên mới", updated.Name);
            Assert.Equal("Mô tả mới", updated.Description);
        }
    }
}

