using TaskManagementSystem.Enums;

namespace TaskManagementSystem.Models.Entity
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public CommonStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}