using LirsPortal.Api.Data;
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using LirsPortal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Allow React frontend origin (Day 3 Frontend Integration)
builder.Services.AddCors(options =>
    options.AddPolicy("portal", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<TaxpayerRepository>();
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<PaymentService>();

var app = builder.Build();

// Any unexpected failure returns a safe message; details stay in the log
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = 500;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new ApiError("Something went wrong. Please try again.", "SERVER_ERROR"));
}));

app.UseCors("portal");

app.MapControllers();
app.Run();
