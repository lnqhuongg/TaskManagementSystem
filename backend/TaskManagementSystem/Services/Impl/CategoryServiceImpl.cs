using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Commons;
using TaskManagementSystem.Commons.Exceptions;
using TaskManagementSystem.DTOs.CategoryDTO;
using TaskManagementSystem.Enums;
using TaskManagementSystem.Models;
using TaskManagementSystem.Models.Entity;

namespace TaskManagementSystem.Services.Impl
{
    public class CategoryServiceImpl : ICategoryService
    {
        private readonly ApplicationDBContext Context;
        public CategoryServiceImpl(ApplicationDBContext context)
        {
            Context = context;
        }

        /*
         * Get All
         */
        public async Task<PagedResult<CategoryResponse>> GetAll(int page, int pageSize, string keyword)
        {
            // 1. tìm kiếm trước (nếu mà keyword rỗng thì nó lấy tất cả)
            var query = SearchByKeyword(keyword);

            // 2. đếm tổng số bản ghi sau khi tìm kiếm
            var total = await query.CountAsync();

            // 3. Phân trang
            // LẤY THEO TRANG, mỗi trang nó lấy theo cái pagesize mình khai báo bên trên, pagesize = 5
            // là lấy 5 bản ghi mỗi trang
            var list = await query
                .Skip((page - 1) * pageSize)  // Bỏ qua bao nhiêu? -- ví dụ trang đầu 1 - 1 * 5 = 0 -> lấy từ 1 đến pagesize = 5 
                .Take(pageSize)               // Lấy bao nhiêu?
                .ToListAsync();

            var data = list
                .Select(ToResponse)
                .ToList();

            return new PagedResult<CategoryResponse>
            {
                Data = data,
                Page = page,
                PageSize = pageSize,
                Total = total
            };
        }

        /*
         * Create
         */
        public async Task<CategoryResponse> Create (CreateCategoryRequest request)
        {
            if (await IsCategoryNameExist(request.Name))
            {
                throw new ConflictException($"Category name '{request.Name}' already exists.");
            }

            var categoryEntity = new Category
            {
                Name = request.Name,
                Description = request.Description,
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                Context.Categories.Add(categoryEntity);
                await Context.SaveChangesAsync();
                return ToResponse(categoryEntity);
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Error creating category: {ex.Message}");
            }
        }

        /*
         * Update
         */
        public async Task<CategoryResponse> Update(int id, UpdateCategoryRequest request)
        {
            var category = await Context.Categories.FindAsync(id);

            if (category == null)
            {
                throw new NotFoundException($"Category with id {id} not found.");
            }

            if (await IsCategoryNameExist(request.Name, id))
            {
                throw new ConflictException($"Category name '{request.Name}' already exists.");
            }

            category.Name = request.Name;
            category.Description = request.Description;
            category.Status = request.Status;
            category.UpdatedAt = DateTime.UtcNow;

            try
            {
                await Context.SaveChangesAsync();
                return ToResponse(category);
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Error updating category: {ex.Message}");
            }
        }

        /*
         * Get By Id
         */
        public async Task<CategoryResponse> GetById (int category_id)
        {
            var category = await Context.Categories.FindAsync(category_id);

            if (category == null)
            {
                throw new NotFoundException($"Category with id {category_id} not found.");
            }
            else return ToResponse(category);
        }

        /*
         * Is Existed
         * Hàm dùng cho cả Create với Update
         */
        private async Task<bool> IsCategoryNameExist(string categoryName, int id = 0)
        {
            // id = 0 -> create
            // truyền id vô để kiểm tra trùng lúc sửa ko bị trùng với chính nó
            return await Context.Categories
                .AnyAsync(x =>
                    x.Name == categoryName &&
                    (id == 0 || x.Id != id)
                );
        }

        private IQueryable<Category> SearchByKeyword(string keyword)
        {
            IQueryable<Category> query = Context.Categories;
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword));
            }
            return query;
        }

        private CategoryResponse ToResponse (Category category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Status = category.Status,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}
