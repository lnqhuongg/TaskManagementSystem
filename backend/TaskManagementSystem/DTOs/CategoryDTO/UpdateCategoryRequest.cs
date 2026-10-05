using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Enums;

namespace TaskManagementSystem.DTOs.CategoryDTO
{
    public class UpdateCategoryRequest
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [EnumDataType(typeof(CommonStatus), ErrorMessage = "Invalid category status")]
        public CommonStatus Status { get; set; }
    }
}