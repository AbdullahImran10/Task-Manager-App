

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
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
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        var roleExists = await _context.Roles
        .AnyAsync(r => r.Id == user.RoleId);
        if (!roleExists)
        {
            return BadRequest(new { message = "Invalid Role ID" });
        }
        var emailExists = await _context.Users
        .AnyAsync(u => u.Email == user.Email);
        if (emailExists)
        {
            return BadRequest(new { message = "Email already exist" });
        }
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            user
        );
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User updatedUser)
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
        user.isActive = updatedUser.isActive;
        await _context.SaveChangesAsync();
        return Ok(user);
    }
}