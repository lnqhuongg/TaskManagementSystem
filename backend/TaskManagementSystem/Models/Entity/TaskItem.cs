using TaskManagementSystem.Enums;

namespace TaskManagementSystem.Models.Entity
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskItemStatus Status { get; set; }
        public TaskItemPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public User User { get; set; } = null!;
        public Category Category { get; set; } = null!;
    }
}