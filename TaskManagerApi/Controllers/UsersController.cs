
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs.Users;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AuditLogService _auditLogService;
    public UsersController(AppDbContext context, AuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users
        .Include(u => u.Role)
        .ToListAsync();
        return Ok(users);
    }
    [HttpGet("admin-test")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminTest()
    {
        return Ok(new
        {
            message = "You are authorized as Admin"
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users
        .Include(r => r.Role)
        .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound(new { message = "User Not Found!" });
        }
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<User>> CreateUser(CreateUserDto dto)
    {
        var roleExists = await _context.Roles
        .AnyAsync(r => r.Id == dto.RoleId);
        if (!roleExists)
        {
            return BadRequest(new { message = "Invalid Role ID" });
        }
        var emailExists = await _context.Users
        .AnyAsync(u => u.Email == dto.Email);
        if (emailExists)
        {
            return BadRequest(new { message = "Email already exist" });
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            RoleId = dto.RoleId,
            isActive = true
        };

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(
            user,
            dto.Password
        );

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (adminIdClaim != null)
        {
            var adminId = int.Parse(adminIdClaim.Value);

            await _auditLogService.LogAsync(
                adminId,
                "CreatedUser",
                $"Created user '{user.FirstName} {user.LastName}' with email '{user.Email}'."
            );
        }

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.RoleId,
                user.isActive
            }
        );
    }
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updatedUser)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User Not Found" });
        }
        var roleExists = await _context.Roles
        .AnyAsync(r => r.Id == updatedUser.RoleId);
        if (!roleExists)
        {
            return BadRequest(new { message = "Invalid Role ID" });
        }
        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.Email = updatedUser.Email;
        user.RoleId = updatedUser.RoleId;
        user.isActive = updatedUser.IsActive;
        await _context.SaveChangesAsync();

        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (adminIdClaim != null)
        {
            var adminId = int.Parse(adminIdClaim.Value);

            await _auditLogService.LogAsync(
                adminId,
                "UpdatedUser",
                $"Updated user '{user.FirstName} {user.LastName}' (ID: {user.Id})."
            );
        }

        return Ok(user);
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (adminIdClaim == null)
        {
            return Unauthorized();
        }

        var adminId = int.Parse(adminIdClaim.Value);

        // Prevent admin from deactivating themselves
        if (adminId == id)
        {
            return BadRequest(new
            {
                message = "You cannot deactivate your own account."
            });
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        if (!user.isActive)
        {
            return BadRequest(new
            {
                message = "User is already inactive."
            });
        }

        user.isActive = false;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            adminId,
            "DeactivatedUser",
            $"Deactivated user '{user.FirstName} {user.LastName}' (ID: {user.Id})."
        );

        return Ok(new
        {
            message = "User deactivated successfully."
        });
    }

    [HttpPut("{id}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (adminIdClaim == null)
        {
            return Unauthorized();
        }

        var adminId = int.Parse(adminIdClaim.Value);

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found."
            });
        }

        if (user.isActive)
        {
            return BadRequest(new
            {
                message = "User is already active."
            });
        }

        user.isActive = true;

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            adminId,
            "ActivatedUser",
            $"Activated user '{user.FirstName} {user.LastName}' (ID: {user.Id})."
        );

        return Ok(new
        {
            message = "User activated successfully."
        });
    }
}