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

        /// <summary>
        /// PHƯƠNG PHÁP TỐI ƯU: Dùng Include (Eager Loading)
        /// Sử dụng .Include(t => t.User).Include(t => t.Category) để EF Core tạo câu lệnh SQL JOIN.
        /// Toàn bộ dữ liệu Task, User, Category được nạp trong 1 query duy nhất!
        /// </summary>
        public async Task<PagedResult<TaskResponse>> GetAll(int page, int pageSize, string keyword)
        {
            var query = Context.TaskItems
                .AsNoTracking() 
                .Include(t => t.User)
                .Include(t => t.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{keyword}%")); // Ilike giúp ko phân biệt hoa thường trong tìm kiếm
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(t => t.CreatedAt)
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

        /// <summary>
        /// TÁI HIỆN LỖI N+1 TRUY VẤN:
        /// 1. Query danh sách TaskItem (1 query).
        /// 2. Duyệt qua từng TaskItem và gửi query riêng biệt để lấy User (N queries) và Category (N queries).
        /// => Tổng cộng: 1 (count) + 1 (tasks) + N (user) + N (category) = 2 + 2N queries xuống database!
        /// Ví dụ pageSize = 5 => phát sinh 12 queries; pageSize = 10 => phát sinh 22 queries.
        /// </summary>
        public async Task<PagedResult<TaskResponse>> GetAllNPlusOne(int page, int pageSize, string keyword)
        {
            // 1. Chỉ query danh sách TaskItems, KHÔNG dùng Include
            var query = Context.TaskItems
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{keyword}%"));
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 2. VÒNG LẶP GÂY N+1: Với mỗi task, tiếp tục query riêng User và Category từ database
            var data = new List<TaskResponse>();
            foreach (var task in list)
            {
                // Query riêng User cho task hiện tại (N queries)
                var user = await Context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == task.UserId);

                // Query riêng Category cho task hiện tại (N queries)
                var category = await Context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == task.CategoryId);

                data.Add(new TaskResponse
                {
                    Id = task.Id,
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    Priority = task.Priority,
                    DueDate = task.DueDate,
                    UserId = task.UserId,
                    UserName = user?.Username ?? string.Empty,
                    CategoryId = task.CategoryId,
                    CategoryName = category?.Name ?? string.Empty,
                    CreatedAt = task.CreatedAt,
                    UpdatedAt = task.UpdatedAt
                });
            }

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
                .AsNoTracking()
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
                DueDate = ToUtc(request.DueDate),
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
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new BadRequestException($"Error creating task: {errorMsg}");
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
            task.DueDate = ToUtc(request.DueDate);
            task.UpdatedAt = DateTime.UtcNow;

            try
            {
                await Context.SaveChangesAsync();

                // reload with navigation properties for response
                var updated = await Context.TaskItems
                    .AsNoTracking()
                    .Include(t => t.User)
                    .Include(t => t.Category)
                    .FirstOrDefaultAsync(t => t.Id == id);

                return ToResponse(updated!);
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new BadRequestException($"Error updating task: {errorMsg}");
            }
        }

        private static DateTime? ToUtc(DateTime? dateTime)
        {
            if (!dateTime.HasValue) return null;
            return dateTime.Value.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc),
                DateTimeKind.Local => dateTime.Value.ToUniversalTime(),
                _ => dateTime.Value
            };
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
