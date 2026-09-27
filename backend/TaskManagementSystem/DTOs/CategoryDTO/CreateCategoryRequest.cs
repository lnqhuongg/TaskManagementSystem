using TaskManagementSystem.Enums;

namespace TaskManagementSystem.DTOs.CategoryDTO
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
