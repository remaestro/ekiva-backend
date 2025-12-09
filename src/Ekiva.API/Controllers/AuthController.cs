using Ekiva.Application.DTOs.Auth;
using Ekiva.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SystemClaim = System.Security.Claims.Claim;
using ClaimTypes = System.Security.Claims.ClaimTypes;

namespace Ekiva.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "User with this email already exists" });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            BranchId = request.BranchId,
            SubsidiaryId = request.SubsidiaryId,
            EmailConfirmed = true // Auto-confirm for now
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, "User");

        var response = await GenerateAuthResponse(user);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        Console.WriteLine($"Login attempt for email: {request.Email}");
        
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            Console.WriteLine($"User not found: {request.Email}");
            return Unauthorized(new { message = "Invalid email or password" });
        }

        Console.WriteLine($"User found: {user.Email}, IsActive: {user.IsActive}");

        if (!user.IsActive)
        {
            Console.WriteLine($"User account is deactivated: {request.Email}");
            return Unauthorized(new { message = "Account is deactivated" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        
        Console.WriteLine($"Password check result - Succeeded: {result.Succeeded}, IsLockedOut: {result.IsLockedOut}, IsNotAllowed: {result.IsNotAllowed}, RequiresTwoFactor: {result.RequiresTwoFactor}");
        
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var response = await GenerateAuthResponse(user);
        Console.WriteLine($"Login successful for: {user.Email}");
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var user = await _userManager.FindByNameAsync(User.Identity!.Name!);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Roles = roles.ToList(),
            BranchId = user.BranchId,
            SubsidiaryId = user.SubsidiaryId
        });
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await _userManager.FindByNameAsync(User.Identity!.Name!);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.BranchId = request.BranchId;
        user.SubsidiaryId = request.SubsidiaryId;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Roles = roles.ToList(),
            BranchId = user.BranchId,
            SubsidiaryId = user.SubsidiaryId
        });
    }

    private async Task<AuthResponse> GenerateAuthResponse(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        // Generate JWT token
        var claims = new List<SystemClaim>
        {
            new SystemClaim(ClaimTypes.NameIdentifier, user.Id),
            new SystemClaim(ClaimTypes.Email, user.Email!),
            new SystemClaim(ClaimTypes.Name, user.UserName!),
            new SystemClaim("firstName", user.FirstName ?? ""),
            new SystemClaim("lastName", user.LastName ?? "")
        };

        foreach (var role in roles)
        {
            claims.Add(new SystemClaim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "EkivaAPI",
            audience: _configuration["Jwt:Audience"] ?? "EkivaClient",
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = Guid.NewGuid().ToString(), // Simplified refresh token
            ExpiresIn = 28800, // 8 hours in seconds
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                FullName = user.FullName,
                Roles = roles.ToList(),
                BranchId = user.BranchId,
                SubsidiaryId = user.SubsidiaryId
            }
        };
    }
}

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public Guid? SubsidiaryId { get; set; }
}
