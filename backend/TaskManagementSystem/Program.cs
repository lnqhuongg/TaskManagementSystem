using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Commons;
using TaskManagementSystem.Infrastructure.DI;
using TaskManagementSystem.Infrastructure.Performance;
using TaskManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

/* 
 * Connect to Database with Query Counter Interceptor
 */
builder.Services.AddSingleton<QueryCounterInterceptor>();
builder.Services.AddDbContext<ApplicationDBContext>((sp, options) =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TaskManagementSystemDatabase")
    );
    options.AddInterceptors(sp.GetRequiredService<QueryCounterInterceptor>());
});

/*
 * In-Memory Cache (IMemoryCache)
 */
builder.Services.AddMemoryCache();

/*
 * Regis all Services from folder Infrastructure
 */
builder.Services.AddApplicationServices();

/*
 * Regis all Controllers & JsonStringEnumConverter 
 */
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });


/*
 * Regis Global Exception Handler 
 */
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

<<<<<<< Updated upstream
=======
/*
 * Regis and config CORS
 */
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Query-Count", "X-Response-Time-Ms", "X-Cache"));
});

>>>>>>> Stashed changes
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middileware HTTPS Redirection
app.UseHttpsRedirection();

<<<<<<< Updated upstream
=======
// Middleware CORS
// Cho phép frontend tại http://localhost:5173 gửi request đến backend, đồng thời quy định các HTTP method và header được phép sử dụng.
app.UseCors("Frontend");

// Middleware đo lường hiệu năng: Ghi nhận số truy vấn DB và thời gian phản hồi qua response header + console log
app.UseMiddleware<PerformanceMiddleware>();

>>>>>>> Stashed changes
// Middileware Global Exception Handler
app.UseExceptionHandler();

// Middileware Authetication


//app.UseHttpsRedirection();

// Middileware Authorization
app.UseAuthorization();



app.MapControllers();

/*
 * Tự động tạo tài khoản Admin mặc định nếu chưa có trong DB
 * Email: admin@example.com | Mật khẩu: Admin@123456
 */
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
    var admin = dbContext.Users.FirstOrDefault(u => u.Email == "admin@example.com");
    if (admin == null)
    {
        dbContext.Users.Add(new TaskManagementSystem.Models.Entity.User
        {
            Username = "admin",
            Email = "admin@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
            Role = TaskManagementSystem.Enums.UserRole.Admin,
            Status = TaskManagementSystem.Enums.CommonStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        dbContext.SaveChanges();
    }
    else
    {
        admin.Password = BCrypt.Net.BCrypt.HashPassword("Admin@123456");
        admin.Status = TaskManagementSystem.Enums.CommonStatus.Active;
        dbContext.SaveChanges();
    }
}

app.Run();
