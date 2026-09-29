using TaskManagementSystem.Enums;

namespace TaskManagementSystem.DTOs.UserDTO
{
    public class UpdateUserRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public CommonStatus Status { get; set; }
    }
}
