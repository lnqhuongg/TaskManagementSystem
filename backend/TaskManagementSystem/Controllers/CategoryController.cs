using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DTOs.CategoryDTO;
using TaskManagementSystem.Services;

namespace TaskManagementSystem.Controllers
{
    [ApiController]
    [Route("categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        /*
         * Get All Categories with Pagination and Keyword Search
         */
        [HttpGet]
        public async Task<IActionResult> GetAll(
            int page = 1,
            int pageSize = 5,
            string keyword = "")
        {
            var categories = await categoryService.GetAll(
                page,
                pageSize,
                keyword);

            return Ok(categories);
        }

        /*
         * Get a Category by ID
         */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await categoryService.GetById(id);

            return Ok(category);
        }

        /*
         * Create a new Category
         */
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateCategoryRequest request)
        {
            Console.WriteLine(">>> CREATE CATEGORY WAS CALLED");
            var category = await categoryService.Create(request);

            

            //return CreatedAtAction(
            //    nameof(GetById),
            //    new { id = category.Id },
            //    category);
            return Ok(category);
        }

        /*
         * Update an existing Category by ID
         */
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCategoryRequest request)
        {
            var category = await categoryService.Update(id, request);

            return Ok(category);
        }
    }
}
