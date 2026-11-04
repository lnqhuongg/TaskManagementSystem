using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Commons.Exceptions;
using TaskManagementSystem.DTOs.TaskDTO;
using TaskManagementSystem.Enums;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("tasks")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskItemService taskService;

        public TaskController(ITaskItemService taskService)
        {
            this.taskService = taskService;
        }

        /// <summary>
        /// Endpoint tối ưu bằng INCLUDE (Eager Loading):
        /// Chỉ sinh 1 query (hoặc 2 kèm count), nạp toàn bộ User và Category bằng JOIN.
        /// Xem Header 'X-Query-Count' và 'X-Response-Time-Ms'.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 5, string keyword = "")
        {
            var tasks = await taskService.GetAll(page, pageSize, keyword, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(tasks);
        }

        /// <summary>
        /// Endpoint DEMO TÁI HIỆN LỖI N+1:
        /// Không dùng Include, duyệt từng Task và query lặp User + Category.
        /// Số query = 2 + 2 * pageSize (ví dụ pageSize=5 sinh 12 queries; pageSize=10 sinh 22 queries).
        /// Xem Header 'X-Query-Count' và 'X-Response-Time-Ms' để so sánh với GET /tasks.
        /// </summary>
        [HttpGet("n-plus-one")]
        public async Task<IActionResult> GetAllNPlusOne(int page = 1, int pageSize = 5, string keyword = "")
        {
            var tasks = await taskService.GetAllNPlusOne(page, pageSize, keyword, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await taskService.GetById(id, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            var task = await taskService.Create(request, GetCurrentUserId(), GetCurrentUserRole());
            return CreatedAtAction(
               nameof(GetById),
               new { id = task.Id },
               task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
        {
            var task = await taskService.Update(id, request, GetCurrentUserId(), GetCurrentUserRole());
            return Ok(task);
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedException("User ID claim not found in token.");
        }

        private UserRole GetCurrentUserRole()
        {
            return User.IsInRole(UserRole.Admin.ToString()) ? UserRole.Admin : UserRole.User;
        }
    }
}
