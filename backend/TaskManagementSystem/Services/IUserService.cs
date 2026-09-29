using TaskManagementSystem.Commons;
using TaskManagementSystem.DTOs.UserDTO;

namespace TaskManagementSystem.Services
{
    public interface IUserService
    {
        Task<PagedResult<UserResponse>> GetAll(int page, int pageSize, string keyword);
        Task<UserResponse> GetById(int user_id);
        Task<UserResponse> Create(CreateUserRequest request);
        Task<UserResponse> Update(int id, UpdateUserRequest request);
    }
}
