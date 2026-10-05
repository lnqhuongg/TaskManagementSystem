using Microsoft.AspNetCore.Diagnostics;
using TaskManagementSystem.Commons.Exceptions;

namespace TaskManagementSystem.Commons
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, 
            Exception exception, 
            CancellationToken cancellationToken)
        // cái cancellationToken này là để hủy bỏ tác vụ nếu cần thiết, giúp tránh tình trạng treo hoặc chậm trễ trong xử lý ngoại lệ
        // hiện tại thì chưa dùng tới, nhưng vẫn truyền vào để tuân thủ giao diện IExceptionHandler
        {
            httpContext.Response.StatusCode = exception switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                BadRequestException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            await httpContext.Response.WriteAsJsonAsync(new ApiResponse<object>
            {
                Success = false,
                Message = exception.Message,
                DataDTO = null
            }, cancellationToken);

            return true;
        }
    }
}
