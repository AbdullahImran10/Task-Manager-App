using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs.Tasks;
using TaskManager.Api.Models;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponseDto>> CreateTask(
        CreateTaskDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var createdById = int.Parse(userIdClaim.Value);

        var assignedUser = await _context.Users
            .FindAsync(dto.AssignedToId);

        if (assignedUser == null)
        {
            return BadRequest(new
            {
                message = "Assigned user does not exist."
            });
        }

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            CreatedById = createdById,
            AssignedToId = dto.AssignedToId,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            Status = "Pending",
            Progress = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);

        await _context.SaveChangesAsync();

        var createdTask = await _context.Tasks
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstAsync(t => t.Id == task.Id);

        var response = new TaskResponseDto
        {
            Id = createdTask.Id,
            Title = createdTask.Title,
            Description = createdTask.Description,

            CreatedById = createdTask.CreatedById,
            CreatedByName =
                $"{createdTask.CreatedBy!.FirstName} {createdTask.CreatedBy.LastName}",

            AssignedToId = createdTask.AssignedToId,
            AssignedToName =
                $"{createdTask.AssignedTo!.FirstName} {createdTask.AssignedTo.LastName}",

            Priority = createdTask.Priority,
            Status = createdTask.Status,
            Progress = createdTask.Progress,
            DueDate = createdTask.DueDate,
            CreatedAt = createdTask.CreatedAt,
            UpdatedAt = createdTask.UpdatedAt,
            CompletedAt = createdTask.CompletedAt
        };

        return CreatedAtAction(
            nameof(GetTask),
            new { id = task.Id },
            response
        );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound(new
            {
                message = "Task not found."
            });
        }

        var response = new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,

            CreatedById = task.CreatedById,
            CreatedByName =
                $"{task.CreatedBy!.FirstName} {task.CreatedBy.LastName}",

            AssignedToId = task.AssignedToId,
            AssignedToName =
                $"{task.AssignedTo!.FirstName} {task.AssignedTo.LastName}",

            Priority = task.Priority,
            Status = task.Status,
            Progress = task.Progress,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CompletedAt = task.CompletedAt
        };

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks()
    {
        var tasks = await _context.Tasks
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,

                CreatedById = t.CreatedById,
                CreatedByName =
                    $"{t.CreatedBy!.FirstName} {t.CreatedBy.LastName}",

                AssignedToId = t.AssignedToId,
                AssignedToName =
                    $"{t.AssignedTo!.FirstName} {t.AssignedTo.LastName}",

                Priority = t.Priority,
                Status = t.Status,
                Progress = t.Progress,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CompletedAt = t.CompletedAt
            })
            .ToListAsync();

        return Ok(tasks);
    }
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetMyTasks()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var tasks = await _context.Tasks
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.AssignedToId == userId)
            .Select(t => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,

                CreatedById = t.CreatedById,
                CreatedByName =
                    $"{t.CreatedBy!.FirstName} {t.CreatedBy.LastName}",

                AssignedToId = t.AssignedToId,
                AssignedToName =
                    $"{t.AssignedTo!.FirstName} {t.AssignedTo.LastName}",

                Priority = t.Priority,
                Status = t.Status,
                Progress = t.Progress,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CompletedAt = t.CompletedAt
            })
            .ToListAsync();

        return Ok(tasks);
    }
}