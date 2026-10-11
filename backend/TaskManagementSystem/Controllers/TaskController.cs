using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DTOs.TaskDTO;
using TaskManagementSystem.Models.Entity;
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

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 5, string keyword = "")
        {
            var tasks = await taskService.GetAll(page, pageSize, keyword);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await taskService.GetById(id);
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            var task = await taskService.Create(request);
            return CreatedAtAction(
               nameof(GetById),
               new { id = task.Id },
               task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
        {
            var task = await taskService.Update(id, request);
            return Ok(task);
        }
    }
}
