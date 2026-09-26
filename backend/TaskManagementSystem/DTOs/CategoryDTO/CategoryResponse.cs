using TaskManagementSystem.Enums;
using TaskManagementSystem.Models.Entity;

namespace TaskManagementSystem.DTOs.CategoryDTO
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CommonStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
