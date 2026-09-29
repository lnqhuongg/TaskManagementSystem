using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Commons;
using TaskManagementSystem.Commons.Exceptions;
using TaskManagementSystem.DTOs.UserDTO;
using TaskManagementSystem.Enums;
using TaskManagementSystem.Models;
using TaskManagementSystem.Models.Entity;

namespace TaskManagementSystem.Services.Impl
{
    public class UserServiceImpl : IUserService
    {
        private readonly ApplicationDBContext Context;

        public UserServiceImpl(ApplicationDBContext context)
        {
            Context = context;
        }

        //GetAll
        public async Task<PagedResult<UserResponse>> GetAll(int page, int pageSize, string keyword)
        {
            // 1. tìm kiếm trước (nếu mà keyword rỗng thì nó lấy tất cả)
            var query = SearchByKeyword(keyword);

            // 2. đếm tổng số bản ghi sau khi tìm kiếm
            var total = await query.CountAsync();

            // 3. Phân trang
            // LẤY THEO TRANG, mỗi trang nó lấy theo cái pagesize mình khai báo bên trên, pagesize = 5
            // là lấy 5 bản ghi mỗi trang
            var list = await query
                .Skip((page - 1) * pageSize)  // Bỏ qua bao nhiêu? -- ví dụ trang đầu 1 - 1 * 5 = 0 -> lấy từ 1 đến pagesize = 5 
                .Take(pageSize)               // Lấy bao nhiêu?
                .ToListAsync();

            var data = list
                .Select(ToResponse)
                .ToList();

            return new PagedResult<UserResponse>
            {
                Data = data,
                Page = page,
                PageSize = pageSize,
                Total = total
            };
        }

        public async Task<UserResponse> GetById(int user_id)
        {
            var user = await Context.Users.FindAsync(user_id);

            if (user == null) 
            {
                throw new NotFoundException($"User with id {user_id} not found.");
            }
            else return ToResponse(user);
        }
        public async Task<UserResponse> Create(CreateUserRequest request)
        {

            if (await IsUsernameExist(request.Username))
            {
                throw new ConflictException($"Username '{request.Username}' already exists.");
            }

            if (await IsEmailExist(request.Email))
            {
                throw new ConflictException($"Email '{request.Email}' already exists.");
            }


            var userEntity = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = request.Role,
                Status = CommonStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                Context.Users.Add(userEntity);
                await Context.SaveChangesAsync();
                return ToResponse(userEntity);
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Error creating user: {ex.Message}");
            }
        }
        public async Task<UserResponse> Update(int id, UpdateUserRequest request)
        {
            var user = await Context.Users.FindAsync(id);
            if (user == null) 
            {
                throw new NotFoundException($"User with id {id} not found.");
            }

            if (await IsUsernameExist(request.Username, id))
            {
                throw new ConflictException($"Username '{request.Username}' already exists.");
            }
            if (await IsEmailExist(request.Email, id))
            {
                throw new ConflictException($"Email '{request.Email}' already exists.");
            }
            user.Username = request.Username;
            user.Email = request.Email;
            user.Role = request.Role;
            user.Status = request.Status;
            user.UpdatedAt = DateTime.UtcNow;
            try
            {
                await Context.SaveChangesAsync();
                return ToResponse(user);
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Error updating user: {ex.Message}");
            }
        }

        private async Task<bool> IsUsernameExist(string username, int id = 0)
        {
            return await Context.Users
                .AnyAsync(x =>
                    x.Username == username &&
                    (id == 0 || x.Id != id)
                );
        }
 
        private async Task<bool> IsEmailExist(string email, int id = 0)
        {
            return await Context.Users
                .AnyAsync(x =>
                    x.Email == email &&
                    (id == 0 || x.Id != id)
                );
        }

        private IQueryable<User> SearchByKeyword(string keyword)
        {
            IQueryable<User> query = Context.Users;
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Username.Contains(keyword) || x.Email.Contains(keyword));
            }
            return query;
        }

        private UserResponse ToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
