using TaskManagementSystem.Enums;

namespace TaskManagementSystem.DTOs.CategoryDTO
{
    public class UpdateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CommonStatus Status { get; set; }
    }
}