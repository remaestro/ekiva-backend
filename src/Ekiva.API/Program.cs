using Ekiva.Core.Interfaces;
using Ekiva.Infrastructure.Data;
using Ekiva.Infrastructure.Repositories;
using Ekiva.Application.Interfaces;
using Ekiva.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ekiva.Core.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Database Configuration
builder.Services.AddDbContext<EkivaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity Configuration with ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<EkivaDbContext>()
    .AddDefaultTokenProviders();

// 3. JWT Authentication
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"))
    };
});

// 4. Dependency Injection
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IRatingService, Ekiva.Application.Services.RatingService>();
builder.Services.AddScoped<IPolicyService, Ekiva.Application.Services.PolicyService>();

// Motor Insurance Services (from Infrastructure)
builder.Services.AddScoped<IMotorPremiumCalculationService, Ekiva.Infrastructure.Services.MotorPremiumCalculationService>();
builder.Services.AddScoped<IMotorQuoteService, Ekiva.Infrastructure.Services.MotorQuoteService>();
builder.Services.AddScoped<IMotorPolicyService, Ekiva.Infrastructure.Services.MotorPolicyService>();
builder.Services.AddScoped<IPdfGenerationService, Ekiva.Infrastructure.Services.PdfGenerationService>();

// Commission & Tax Services (Phase 8)
builder.Services.AddScoped<ICommissionCalculator, CommissionCalculator>();
builder.Services.AddScoped<ITaxCalculator, TaxCalculator>();

// ASACI Service (Phase 9)
builder.Services.AddScoped<IASACIService, ASACIService>();

// Admin Service (Phase 10)
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddAutoMapper(typeof(Ekiva.Application.Mappings.MappingProfile));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularDev");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Removed MapIdentityApi - using custom AuthController instead

// Seed Database and Roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.SeedAsync(services);
        await SeedRolesAndAdminUser(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

// Helper method to seed roles and admin user
static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    // Define roles
    string[] roles = { "Admin", "Manager", "Broker", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            logger.LogInformation($"Role '{role}' created successfully");
        }
    }

    // Create or update admin user
    var adminEmail = "admin@ekiva.com";
    var adminPassword = "Admin@123";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        // Create new admin user
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "EKIVA",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation($"Admin user created successfully with email: {adminEmail}");
        }
        else
        {
            logger.LogError($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        // User exists, reset password to ensure it's correct
        logger.LogInformation($"Admin user already exists. Resetting password...");
        
        var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
        var resetResult = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
        
        if (resetResult.Succeeded)
        {
            logger.LogInformation("Admin password reset successfully");
            
            // Ensure user is active and in Admin role
            if (!adminUser.IsActive)
            {
                adminUser.IsActive = true;
                await userManager.UpdateAsync(adminUser);
            }
            
            var userRoles = await userManager.GetRolesAsync(adminUser);
            if (!userRoles.Contains("Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            logger.LogError($"Failed to reset admin password: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
        }
    }
}