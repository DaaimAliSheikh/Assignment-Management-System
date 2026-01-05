using AssignmentManagementSystem.Data;
using AssignmentManagementSystem.Models;
using AssignmentManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Assignment Management System API",
        Version = "v1",
        Description = "API for managing assignments, classrooms, and submissions"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireTeacher", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("RequireStudent", policy => policy.RequireRole("Student"));
});

// Register application services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// Configure CORS (optional, for frontend integration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed roles and create default users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        
        // Seed roles
        string[] roleNames = { "Teacher", "Student" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        
        // Optionally create a default teacher account for testing
        var defaultTeacherEmail = "teacher@test.com";
        var defaultTeacher = await userManager.FindByEmailAsync(defaultTeacherEmail);
        if (defaultTeacher == null)
        {
            var teacher = new ApplicationUser
            {
                UserName = defaultTeacherEmail,
                Email = defaultTeacherEmail,
                EmailConfirmed = true,
                FullName = "Default Teacher",
                Age = 30,
                Gender = "Male",
                Description = "Default teacher account for testing"
            };
            
            var result = await userManager.CreateAsync(teacher, "Teacher@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(teacher, "Teacher");
            }
        }
        
        // Optionally create a default student account for testing
        var defaultStudentEmail = "student@test.com";
        var defaultStudent = await userManager.FindByEmailAsync(defaultStudentEmail);
        if (defaultStudent == null)
        {
            var student = new ApplicationUser
            {
                UserName = defaultStudentEmail,
                Email = defaultStudentEmail,
                EmailConfirmed = true,
                FullName = "Default Student",
                Age = 20,
                Gender = "Female",
                Description = "Default student account for testing"
            };
            
            var result = await userManager.CreateAsync(student, "Student@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(student, "Student");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Assignment Management System API V1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
