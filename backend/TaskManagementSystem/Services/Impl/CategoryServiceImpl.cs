using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TaskManagementSystem.Commons;
using TaskManagementSystem.Commons.Exceptions;
using TaskManagementSystem.DTOs.CategoryDTO;
using TaskManagementSystem.Enums;
using TaskManagementSystem.Infrastructure.Performance;
using TaskManagementSystem.Models;
using TaskManagementSystem.Models.Entity;

namespace TaskManagementSystem.Services.Impl
{
    public class CategoryServiceImpl : ICategoryService
    {
        private readonly ApplicationDBContext Context;
        private readonly IMemoryCache _cache;

        // Cache keys
        private const string ActiveCategoriesCacheKey = "categories_active";
        private static string GetCategoryCacheKey(int id) => $"category_{id}";

        public CategoryServiceImpl(ApplicationDBContext context, IMemoryCache cache)
        {
            Context = context;
            _cache = cache;
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

                // XÓA CACHE: Đảm bảo lần gọi GetAllActiveCategories tiếp theo sẽ lấy dữ liệu mới nhất
                _cache.Remove(ActiveCategoriesCacheKey);

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

                // XÓA CACHE: Xóa cả cache danh sách active và cache chi tiết của id này
                _cache.Remove(ActiveCategoriesCacheKey);
                _cache.Remove(GetCategoryCacheKey(id));

                return ToResponse(category);
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Error updating category: {ex.Message}");
            }
        }

        /*
         * Get By Id (Áp dụng IMemoryCache)
         */
        public async Task<CategoryResponse> GetById (int category_id)
        {
            var cacheKey = GetCategoryCacheKey(category_id);

            // 1. Kiểm tra cache (Cache Hit: 0 truy vấn DB)
            if (_cache.TryGetValue(cacheKey, out CategoryResponse? cachedCategory) && cachedCategory != null)
            {
                PerformanceTracker.MarkCacheHit();
                return cachedCategory;
            }

            // 2. Cache Miss: Truy vấn Database
            var category = await Context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == category_id);

            if (category == null)
            {
                throw new NotFoundException($"Category with id {category_id} not found.");
            }

            var response = ToResponse(category);

            // 3. Ghi vào MemoryCache (Sliding 5 phút, Absolute 30 phút)
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

            _cache.Set(cacheKey, response, cacheOptions);

            return response;
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
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Name == categoryName &&
                    (id == 0 || x.Id != id)
                );
        }

        private IQueryable<Category> SearchByKeyword(string keyword)
        {
            IQueryable<Category> query = Context.Categories.AsNoTracking();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword));
            }
            return query;
        }

        /*
         * Get All Active Categories (Áp dụng IMemoryCache)
         * Danh mục công việc ít biến động, là ứng viên số 1 để cache
         */
        public async Task<List<CategoryResponse>> GetAllActiveCategories()
        {
            // 1. Kiểm tra cache (Cache Hit: 0 truy vấn DB, phản hồi < 1ms)
            if (_cache.TryGetValue(ActiveCategoriesCacheKey, out List<CategoryResponse>? cachedCategories) && cachedCategories != null)
            {
                PerformanceTracker.MarkCacheHit();
                return cachedCategories;
            }

            // 2. Cache Miss: Truy vấn từ Database với AsNoTracking
            var activeCategories = await Context.Categories
                .AsNoTracking()
                .Where(c => c.Status == CommonStatus.Active)
                .ToListAsync();

            var result = activeCategories.Select(ToResponse).ToList();

            // 3. Ghi vào MemoryCache
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

            _cache.Set(ActiveCategoriesCacheKey, result, cacheOptions);

            return result;
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
