using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs.Notifications;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>>
        GetNotifications()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                TaskId = n.TaskId
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("unread")]
    public async Task<ActionResult<IEnumerable<NotificationResponseDto>>>
        GetUnreadNotifications()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                TaskId = n.TaskId
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPut("{id}/read")]
public async Task<IActionResult> MarkAsRead(int id)
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

    if (userIdClaim == null)
    {
        return Unauthorized();
    }

    var userId = int.Parse(userIdClaim.Value);

    var notification = await _context.Notifications
        .FirstOrDefaultAsync(n =>
            n.Id == id &&
            n.UserId == userId);

    if (notification == null)
    {
        return NotFound(new
        {
            message = "Notification not found."
        });
    }

    notification.IsRead = true;

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Notification marked as read."
    });
}

[HttpGet("unread-count")]
public async Task<IActionResult> GetUnreadCount()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

    if (userIdClaim == null)
    {
        return Unauthorized();
    }

    var userId = int.Parse(userIdClaim.Value);

    var count = await _context.Notifications
        .CountAsync(n =>
            n.UserId == userId &&
            !n.IsRead);

    return Ok(new
    {
        count
    });
}
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "All notifications marked as read."
        });
    }
}

