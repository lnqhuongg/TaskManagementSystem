using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Enums;

namespace TaskManagementSystem.DTOs.TaskDTO
{
    public class CreateTaskRequest
    {
        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Task title must be between 3 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [EnumDataType(typeof(TaskItemStatus), ErrorMessage = "Invalid task status")]
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

        [EnumDataType(typeof(TaskItemPriority), ErrorMessage = "Invalid task priority")]
        public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;

        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be a positive integer")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "CategoryId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be a positive integer")]
        public int CategoryId { get; set; }
    }
}
