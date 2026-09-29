using TaskManagementSystem.Enums;

namespace TaskManagementSystem.DTOs.TaskDTO
{
    public class UpdateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
        public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;
        public DateTime? DueDate { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
    }
}
