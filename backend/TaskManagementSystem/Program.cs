using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Commons;
using TaskManagementSystem.Infrastructure.DI;
using TaskManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

/* 
 * Connect to Database
 */
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TaskManagementSystemDatabase")
    )
);

/*
 * Regis all Services from folder Infrastructure
 */
builder.Services.AddApplicationServices();

builder.Services.AddJwtAuthentication(builder.Configuration);


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

/*
 * Regis and config CORS
 */
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middileware HTTPS Redirection
// Tự động chuyển các HTTP request sang HTTPS để tăng tính bảo mật khi truyền dữ liệu.
app.UseHttpsRedirection();

// Middleware CORS
// Cho phép frontend tại http://localhost:5173 gửi request đến backend, đồng thời quy định các HTTP method và header được phép sử dụng.
app.UseCors("Frontend");

// Middileware Global Exception Handler
// Xử lý các exception phát sinh trong quá trình xử lý request, chuyển chúng đến GlobalExceptionHandler để trả về response lỗi thống nhất.
app.UseExceptionHandler();

// Middileware Authetication
// Xác thực danh tính người dùng dựa trên thông tin xác thực, chẳng hạn JWT token được gửi trong request.
app.UseAuthentication();

// Middileware Authorization
// Kiểm tra quyền truy cập của người dùng đối với endpoint dựa trên các chính sách hoặc thuộc tính như [Authorize] và [Authorize(Roles = "Admin")].
app.UseAuthorization();

app.MapControllers();

app.Run();
