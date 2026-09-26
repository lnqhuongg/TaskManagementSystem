using TaskManagementSystem.Commons;
using TaskManagementSystem.DTOs.CategoryDTO;

namespace TaskManagementSystem.Services
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryResponse>> GetAll(int page, int pageSize, string keyword);
        Task<CategoryResponse> GetById(int category_id);
        Task<CategoryResponse> Create(CreateCategoryRequest request);
        Task<CategoryResponse> Update(int id, UpdateCategoryRequest request);
    }
}
