using TaskManagementSystem.Commons;
using TaskManagementSystem.DTOs.TaskDTO;

namespace TaskManagementSystem.Services
{
    public interface ITaskItemService
    {
        Task<PagedResult<TaskResponse>> GetAll(int page, int pageSize, string keyword);
        Task<TaskResponse> GetById(int id);
        Task<TaskResponse> Create(CreateTaskRequest request);
        Task<TaskResponse> Update(int id, UpdateTaskRequest request);
    }
}
