using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

        // Trong ServiceCollectionExtension.cs
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key is missing in appsettings.json");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidAudience = jwtSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }

    }
}