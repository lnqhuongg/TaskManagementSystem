using TaskManagementSystem.Commons;
using TaskManagementSystem.DTOs.TaskDTO;

namespace TaskManagementSystem.Services
{
    public interface ITaskItemService
    {
        // 1. Tối ưu bằng Include (Eager Loading)
        Task<PagedResult<TaskResponse>> GetAll(int page, int pageSize, string keyword);

        // 2. Tái hiện lỗi N+1 (truy vấn lặp từng bản ghi)
        Task<PagedResult<TaskResponse>> GetAllNPlusOne(int page, int pageSize, string keyword);

        Task<TaskResponse> GetById(int id);
        Task<TaskResponse> Create(CreateTaskRequest request);
        Task<TaskResponse> Update(int id, UpdateTaskRequest request);
    }
}
