using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Commons;
using TaskManagementSystem.Commons.Exceptions;
using TaskManagementSystem.DTOs.TaskDTO;
using TaskManagementSystem.Enums;
using TaskManagementSystem.Models;
using TaskManagementSystem.Models.Entity;

namespace TaskManagementSystem.Services.Impl
{
    public class TaskItemServiceImpl : ITaskItemService
    {
        private readonly ApplicationDBContext Context;

        public TaskItemServiceImpl(ApplicationDBContext context)
        {
            Context = context;
        }

        public async Task<PagedResult<TaskResponse>> GetAll(int page, int pageSize, string keyword)
        {
            var query = Context.TaskItems
                .Include(t => t.User)
                .Include(t => t.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{keyword}%")); // Ilike giúp ko phân biệt hoa thường trong tìm kiếm
            }

            var total = await query.CountAsync();

            var list = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var data = list.Select(ToResponse).ToList();

            return new PagedResult<TaskResponse>
            {
                Data = data,
                Page = page,
                PageSize = pageSize,
                Total = total
            };
        }

        public async Task<TaskResponse> GetById(int id)
        {
            var task = await Context.TaskItems
                .Include(t => t.User)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                throw new NotFoundException($"Task with id {id} not found.");
            }

            return ToResponse(task);
        }

        public async Task<TaskResponse> Create(CreateTaskRequest request)
        {
            var user = await Context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException($"User with id {request.UserId} not found.");
            }

            var category = await Context.Categories.FindAsync(request.CategoryId);
            if (category == null)
            {
                throw new NotFoundException($"Category with id {request.CategoryId} not found.");
            }

            var taskEntity = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                Priority = request.Priority,
                DueDate = request.DueDate,
                UserId = request.UserId,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                User = user,
                Category = category
            };

            try
            {
                Context.TaskItems.Add(taskEntity);
                await Context.SaveChangesAsync();
                return ToResponse(taskEntity);
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Error creating task: {ex.Message}");
            }
        }

        public async Task<TaskResponse> Update(int id, UpdateTaskRequest request)
        {
            var task = await Context.TaskItems.FindAsync(id);

            if (task == null)
            {
                throw new NotFoundException($"Task with id {id} not found.");
            }

            if (task.UserId != request.UserId)
            {
                var user = await Context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    throw new NotFoundException($"User with id {request.UserId} not found.");
                }
                task.UserId = request.UserId;
            }

            if (task.CategoryId != request.CategoryId)
            {
                var category = await Context.Categories.FindAsync(request.CategoryId);
                if (category == null)
                {
                    throw new NotFoundException($"Category with id {request.CategoryId} not found.");
                }
                task.CategoryId = request.CategoryId;
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = request.Status;
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            try
            {
                await Context.SaveChangesAsync();

                // reload with navigation properties for response
                var updated = await Context.TaskItems
                    .Include(t => t.User)
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => t.Id == id);

                return ToResponse(updated!);
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Error updating task: {ex.Message}");
            }
        }

        private TaskResponse ToResponse(TaskItem task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                UserId = task.UserId,
                UserName = task.User?.Username ?? string.Empty,
                CategoryId = task.CategoryId,
                CategoryName = task.Category?.Name ?? string.Empty,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}
