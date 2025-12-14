using Ekiva.Core.Interfaces;
using Ekiva.Infrastructure.Data;
using Ekiva.Infrastructure.Repositories;
using Ekiva.Application.Interfaces;
using Ekiva.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Ekiva.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Database Configuration
builder.Services.AddDbContext<EkivaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity Configuration with ApplicationUser
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EkivaDbContext>();

// 3. Dependency Injection
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
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://black-plant-0769c4603.azurestaticapps.net",
                "https://black-plant-0769c4603.1.azurestaticapps.net",
                "https://black-plant-0769c4603.2.azurestaticapps.net",
                "https://black-plant-0769c4603.3.azurestaticapps.net"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition", "Authorization")
              .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
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

// IMPORTANT: CORS must come before UseHttpsRedirection and Authentication
app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<ApplicationUser>(); // Basic Identity Endpoints with ApplicationUser

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

    // Define roles
    string[] roles = { "Admin", "Manager", "Broker", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Create admin user if not exists
    var adminEmail = "admin@ekiva.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "EKIVA",
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}