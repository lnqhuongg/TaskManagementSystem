using TaskManagementSystem.Commons;
using TaskManagementSystem.DTOs.TaskDTO;
using TaskManagementSystem.Enums;

namespace TaskManagementSystem.Services
{
    public interface ITaskItemService
    {
        // 1. Tối ưu bằng Include (Eager Loading)
        Task<PagedResult<TaskResponse>> GetAll(int page, int pageSize, string keyword, int currentUserId, UserRole currentUserRole);

        // 2. Tái hiện lỗi N+1 (truy vấn lặp từng bản ghi)
        Task<PagedResult<TaskResponse>> GetAllNPlusOne(int page, int pageSize, string keyword, int currentUserId, UserRole currentUserRole);

        Task<TaskResponse> GetById(int id, int currentUserId, UserRole currentUserRole);
        Task<TaskResponse> Create(CreateTaskRequest request, int currentUserId, UserRole currentUserRole);
        Task<TaskResponse> Update(int id, UpdateTaskRequest request, int currentUserId, UserRole currentUserRole);
    }
}
