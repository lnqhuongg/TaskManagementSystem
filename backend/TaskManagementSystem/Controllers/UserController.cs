using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManagementSystem.DTOs.UserDTO;
using TaskManagementSystem.Models.Entity;
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 5,
            string keyword = "")
        {
            var users = await userService.GetAll(
                page,
                pageSize,
                keyword);

            return Ok(users);
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await userService.GetById(id);

            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateUserRequest request)
        {
            Console.WriteLine(">>> CREATE USER WAS CALLED");
            var user = await userService.Create(request);

            //return Ok(user);
            return CreatedAtAction(
               nameof(GetById),
               new { id = user.Id },
               user);
        }

        [Authorize]
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
