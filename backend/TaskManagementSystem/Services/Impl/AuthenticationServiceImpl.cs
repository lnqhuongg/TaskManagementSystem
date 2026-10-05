using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManagementSystem.Commons.Exceptions;
using TaskManagementSystem.DTOs.AuthDTO;
using TaskManagementSystem.Enums;
using TaskManagementSystem.Models;
using TaskManagementSystem.Models.Entity;

namespace TaskManagementSystem.Services.Impl
{
    public class AuthenticationServiceImpl : IAuthenticationService
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationServiceImpl(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            // 1. Tìm user theo Email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            // 2. Kiểm tra nếu user không tồn tại hoặc tài khoản bị khóa
            if (user == null || user.Status == CommonStatus.Inactive)
            {
                // Thông báo chung để tránh để lộ thông tin email có tồn tại hay không
                throw new BadRequestException("Invalid email or password.");
            }

            // 3. Kiểm tra mật khẩu bằng BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!isPasswordValid)
            {
                throw new BadRequestException("Invalid email or password.");
            }

            // 4. Đọc thời gian hết hạn của token từ appsettings (mặc định 60 phút nếu chưa cấu hình)
            var durationInMinutes = _configuration.GetValue<int>("Jwt:DurationInMinutes", 60);
            var expiresAt = DateTime.UtcNow.AddMinutes(durationInMinutes);

            // 5. Sinh chuỗi JWT Token
            var token = GenerateJwtToken(user, expiresAt);

            // 6. Trả về kết quả cho Controller
            return new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        private string GenerateJwtToken(User user, DateTime expiresAt)
        {
            // Đọc các giá trị bí mật từ appsettings.json
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is not configured.");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            // Tạo các Claims (Payload của Token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Gán Role để phân quyền [Authorize(Roles = "...")]
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Tạo Token
            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            // Viết token thành chuỗi string (header.payload.signature)
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
