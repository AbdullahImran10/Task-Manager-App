
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs.Users;
using TaskManager.Api.Models;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    public UsersController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users
        .Include(u => u.Role)
        .ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
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
            return BadRequest(new { message = "Invalid Role ID"});
        }
        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.Email = updatedUser.Email;
        user.RoleId = updatedUser.RoleId;
        user.isActive = updatedUser.IsActive;
        await _context.SaveChangesAsync();
        return Ok(user);
    }
}