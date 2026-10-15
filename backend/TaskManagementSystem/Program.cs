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
app.UseHttpsRedirection();

// Middleware CORS
app.UseCors("Frontend");

// Middileware Global Exception Handler
app.UseExceptionHandler();

// Middileware Authetication
app.UseAuthentication();

// Middileware Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
