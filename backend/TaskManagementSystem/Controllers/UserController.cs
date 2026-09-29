using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DTOs.UserDTO;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        public UserController(IUserService userService) 
        { 
            this.userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 5,
            string keyword = "")
        {
            var categories = await userService.GetAll(
                page,
                pageSize,
                keyword);

            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await userService.GetById(id);

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateUserRequest request)
        {
            Console.WriteLine(">>> CREATE USER WAS CALLED");
            var user = await userService.Create(request);

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateUserRequest request)
        {
            var user = await userService.Update(id, request);

            return Ok(user);
        }
    }
}
