using System.Text;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using LirsPortal.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------------------
// 1. Controllers & CORS Configuration
// ------------------------------------------------------------------------------
// Registers ASP.NET Core controller routing.
// CORS policy authorizes our React frontend running on port 5173 to communicate with this API.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
    options.AddPolicy("portal", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ------------------------------------------------------------------------------
// 2. JWT Authentication & Token Validation Middleware
// ------------------------------------------------------------------------------
// Configures JWT Bearer authentication scheme.
// Every request bearing an "Authorization: Bearer <token>" header is automatically intercepted
// and cryptographically verified before reaching any controller endpoint.
var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "LirsSuperSecretDefaultKeyForTrainingSession2026!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LirsPortal",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LirsPortalClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            // ClockSkew zero prevents allowing expired tokens during grace periods
            ClockSkew = TimeSpan.Zero
        };
    });

// ------------------------------------------------------------------------------
// 3. Dependency Injection (DI) Registrations
// ------------------------------------------------------------------------------
// Decoupled architecture following SOLID principles:
// - Singletons: Stateless utility services reused across the entire application lifecycle.
// - Scoped: Instantiated once per incoming HTTP request and disposed after completion.
// - Interface-based abstractions: IPaymentRepository and IPaymentGatewayService enable
//   mocking and automated test isolation.
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<TaxpayerRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentGatewayService, MockPaymentGatewayService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<PenaltyCalculator>();

var app = builder.Build();

// ------------------------------------------------------------------------------
// 4. Global Centralized Exception Handling
// ------------------------------------------------------------------------------
// In enterprise revenue systems, never leak database connection strings or stack traces.
// Unhandled exceptions are safely caught and returned as clean, standardized JSON errors.
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = 500;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new ApiError("Something went wrong. Please try again.", "SERVER_ERROR"));
}));

// ------------------------------------------------------------------------------
// 5. HTTP Middleware Pipeline Execution Order
// ------------------------------------------------------------------------------
// Middleware order is critical in ASP.NET Core:
// 1. CORS allows the browser preflight request.
// 2. Authentication parses and verifies the JWT token.
// 3. Authorization checks user roles and ownership rules.
// 4. MapControllers routes the verified request to the matching controller action.
app.UseCors("portal");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
