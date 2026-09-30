using TaskManagementSystem.Enums;

namespace TaskManagementSystem.DTOs.UserDTO
{
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        //public CommonStatus status { get; set; }
        //public string Password { get; set; } = string.Empty; đợi cập nhật data base
        public UserRole Role { get; set; }
    }
}
