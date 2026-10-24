using System.Security.Claims;
using System.Text;
using HRDirectAPI.Data;
using HRDirectAPI.Helpers;
using HRDirectAPI.Repositories.Implementations;
using HRDirectAPI.Repositories.Interfaces;
using HRDirectAPI.Services.Implementations;
using HRDirectAPI.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITrainingRepository, TrainingRepository>();
builder.Services.AddScoped<ITrainingService, TrainingService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["JWT:Key"]!)),

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var employeeIdClaim = context.Principal?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(employeeIdClaim, out var employeeId))
                {
                    context.Fail("Invalid authentication token.");
                    return;
                }

                var dbContext = context.HttpContext.RequestServices
                    .GetRequiredService<ApplicationDbContext>();

                var employee = await dbContext.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == employeeId);

                if (employee == null || !employee.IsActive)
                {
                    context.Fail("Employee account is inactive or unavailable.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();