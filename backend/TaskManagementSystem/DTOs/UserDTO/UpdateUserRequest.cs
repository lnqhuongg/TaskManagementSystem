using System.ComponentModel.DataAnnotations;
using TaskManagementSystem.Enums;

namespace TaskManagementSystem.DTOs.UserDTO
{
    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and underscores")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid user role")]
        public UserRole Role { get; set; }

        [EnumDataType(typeof(CommonStatus), ErrorMessage = "Invalid user status")]
        public CommonStatus Status { get; set; }
    }
}
