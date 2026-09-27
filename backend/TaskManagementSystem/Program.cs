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

/*
 * Regis all Controllers 
 */
builder.Services.AddControllers();

/*
 * Regis Global Exception Handler 
 */
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
