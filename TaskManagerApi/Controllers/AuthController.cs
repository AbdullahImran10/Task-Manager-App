
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs.Auth;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly AuditLogService _auditLogService;
    public AuthController(AppDbContext context, IConfiguration configuration, AuditLogService auditLogService)
    {
        _context = context;
        _configuration = configuration;
        _auditLogService = auditLogService;
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var user = await _context.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null)
        {
            await _auditLogService.LogAsync(
            null,
            "UserLoginFailed",
            $"Failed login attempt for unknown email: {loginDto.Email}"
);
            return Unauthorized(
                new
                {
                    message = "Invalid email or password"
                }
            );
        }
        if (!user.isActive)
        {
             await _auditLogService.LogAsync(
        user.Id,
        "UserLoginFailed",
        "Login attempt was rejected because the account is inactive."
    );
            return Unauthorized(new
            {
                message = "Your account is inactive"
            }
            );
        }
        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            loginDto.Password
        );
        if (result == PasswordVerificationResult.Failed)
        {
             await _auditLogService.LogAsync(
                user.Id,
                "UserLoginFailed",
                "Failed login attempt due to incorrect password."
    );
            return Unauthorized(new
            {
                message = "Invalid email or password"
            });
        }
        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
        user.Id,
            "UserLogin",
            $"User {user.FirstName} {user.LastName} logged in successfully."
        );

        var token = GenerateToken(user);
        return Ok(new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            FirstName = user.FirstName,
            Email = user.Email,
            Role = user.Role?.Name ?? string.Empty
        });
    }
    private string GenerateToken(User user)
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException(
                "Jwt Key is not configured"
            );
        }

        var claims = new List<Claim>
        {
            new Claim(
             ClaimTypes.NameIdentifier,
             user.Id.ToString()
            ),
            new Claim(
            ClaimTypes.Name,
            user.FirstName
            ),
            new Claim(
              ClaimTypes.Email,
              user.Email
            ),
            new Claim(
              ClaimTypes.Role,
              user.Role?.Name ?? "Employee"
            ),
        };
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key)
            );
        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
