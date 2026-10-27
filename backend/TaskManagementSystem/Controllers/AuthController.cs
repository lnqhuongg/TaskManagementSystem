using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DTOs.AuthDTO;
using TaskManagementSystem.DTOs.UserDTO;
using TaskManagementSystem.Enums;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthenticationService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.Login(request);
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
        {
            // Tận dụng lại logic tạo user từ IUserService
            // Ép role cố định là User để người dùng đăng ký không thể tự cấp quyền Admin
            request.Role = UserRole.User;
            var response = await _userService.Create(request);
            return Ok(response);
        }
    }
}
