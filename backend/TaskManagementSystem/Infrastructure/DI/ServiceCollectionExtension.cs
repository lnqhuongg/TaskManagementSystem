using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.Services;
using TaskManagementSystem.Services.Impl;

namespace TaskManagementSystem.Infrastructure.DI
{
    public static class ServiceCollectionExtension
    {
        /* 
         * IServiceCollection la giao dien co san trong ASP.NET Core 
         * de dang ky cac dich vu (services) cho ung dung 
         */
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Đăng ký tất cả service ở đây
            services.AddScoped<ICategoryService, CategoryServiceImpl>();
            services.AddScoped<ITaskItemService, TaskItemServiceImpl>();
            services.AddScoped<IUserService, UserServiceImpl>();
            services.AddScoped<IAuthenticationService, AuthenticationServiceImpl>();

            return services;
        }
    }
}