using TaskManagementSystem.DTOs.AuthDTO;

namespace TaskManagementSystem.Services
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> Login(LoginRequest request);
    }
}
