using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs.Tasks;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AuditLogService _auditLogService;

    public TasksController(AppDbContext context, AuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
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
     .Include(u => u.Role)
     .FirstOrDefaultAsync(u => u.Id == dto.AssignedToId);

        if (assignedUser == null)
        {
            return BadRequest(new
            {
                message = "Assigned user does not exist."
            });
        }

        if (!assignedUser.isActive)
        {
            return BadRequest(new
            {
                message = "Cannot assign a task to an inactive user."
            });
        }

        if (assignedUser.Role?.Name != "Employee")
        {
            return BadRequest(new
            {
                message = "Tasks can only be assigned to employees."
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

        var notification = new Notification
        {
            UserId = task.AssignedToId,
            TaskId = task.Id,
            Message = $"You have been assigned a new task: {task.Title}",
            Type = "TaskAssigned",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
        createdById,
        "CreatedTask",
        $"Created task '{task.Title}' and assigned it to {assignedUser.FirstName} {assignedUser.LastName}."
        );

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
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(int id, UpdateTaskDto dto)
    {
        var task = await _context.Tasks
                                 .Include(t => t.CreatedBy)
                                 .Include(t => t.AssignedTo)
                                 .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null)
        {
            return NotFound(new
            {
                message = "Task Not Found"
            });
        }
        var previousStatus = task.Status;
        var previousAssignedToId = task.AssignedToId;

        var previousAssignedToName = task.AssignedTo != null
    ? $"{task.AssignedTo.FirstName} {task.AssignedTo.LastName}"
    : "Unknown User";

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }
        var userId = int.Parse(userIdClaim.Value);
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "Employee" && task.AssignedToId != userId)
        {
            return Forbid();
        }
        var validStatuses = new[]
        {
            "Pending",
            "InProgress",
            "Completed",
            "Cancelled"
        };
        if (!validStatuses.Contains(dto.Status))
        {
            return BadRequest(new
            {
                message = "Invalid task status."
            });
        }

        if (dto.Progress < 0 || dto.Progress > 100)
        {
            return BadRequest(new
            {
                message = "Progress must be between 0 and 100."
            });
        }

        if (dto.Status == "Completed" && dto.Progress != 100)
        {
            return BadRequest(new
            {
                message = "A completed task must have 100% progress."
            });
        }

        if (dto.Status == "InProgress" && dto.Progress == 100)
        {
            return BadRequest(new
            {
                message = "An in-progress task cannot have 100% progress."
            });
        }

        if (userRole == "Admin")
        {
            // Admin can modify all task details
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Priority = dto.Priority;
            task.DueDate = dto.DueDate;
            task.Status = dto.Status;
            task.Progress = dto.Progress;
        }
        else if (userRole == "Employee")
        {
            // Employee can only update task progress and status
            task.Status = dto.Status;
            task.Progress = dto.Progress;
        }

        task.UpdatedAt = DateTime.UtcNow;

        if (userRole == "Admin")
        {
            var assignedUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == dto.AssignedToId);

            if (assignedUser == null)
            {
                return BadRequest(new
                {
                    message = "Assigned user does not exist."
                });
            }

            if (!assignedUser.isActive)
            {
                return BadRequest(new
                {
                    message = "Cannot assign a task to an inactive user."
                });
            }

            if (assignedUser.Role?.Name != "Employee")
            {
                return BadRequest(new
                {
                    message = "Tasks can only be assigned to employees."
                });
            }

            task.AssignedToId = dto.AssignedToId.Value;
            if (previousAssignedToId != task.AssignedToId)
            {
                var notification = new Notification
                {
                    UserId = task.AssignedToId,
                    TaskId = task.Id,
                    Message = $"You have been assigned a task: {task.Title}",
                    Type = "TaskAssigned",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
            }
        }
        if (dto.Status == "Completed")
        {
            task.Progress = 100;

            if (task.CompletedAt == null)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
        }
        else
        {
            task.CompletedAt = null;
        }
        await _context.SaveChangesAsync();

        if (userRole == "Admin" &&
    previousAssignedToId != task.AssignedToId)
        {
            var newAssignedUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == task.AssignedToId);

            await _auditLogService.LogAsync(
                userId,
                "ReassignedTask",
                $"Reassigned task '{task.Title}' (ID: {task.Id}) " +
                $"from {previousAssignedToName} to " +
                $"{newAssignedUser!.FirstName} {newAssignedUser.LastName}."
            );
        }
        else if (dto.Status == "Completed" &&
             previousStatus != "Completed")
        {
            await _auditLogService.LogAsync(
                userId,
                "CompletedTask",
                $"Completed task '{task.Title}' (ID: {task.Id})."
            );
        }
        else
        {
            await _auditLogService.LogAsync(
                userId,
                "UpdatedTask",
                $"Updated task '{task.Title}' (ID: {task.Id}). " +
                $"Status: {task.Status}, Progress: {task.Progress}%."
            );
        }

        var updatedTask = await _context.Tasks
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstAsync(t => t.Id == task.Id);
        var response = new TaskResponseDto
        {
            Id = updatedTask.Id,
            Title = updatedTask.Title,
            Description = updatedTask.Description,

            CreatedById = updatedTask.CreatedById,
            CreatedByName =
        $"{updatedTask.CreatedBy!.FirstName} {updatedTask.CreatedBy.LastName}",

            AssignedToId = updatedTask.AssignedToId,
            AssignedToName =
        $"{updatedTask.AssignedTo!.FirstName} {updatedTask.AssignedTo.LastName}",

            Priority = updatedTask.Priority,
            Status = updatedTask.Status,
            Progress = updatedTask.Progress,
            DueDate = updatedTask.DueDate,
            CreatedAt = updatedTask.CreatedAt,
            UpdatedAt = updatedTask.UpdatedAt,
            CompletedAt = updatedTask.CompletedAt
        };
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound(new
            {
                message = "Task not found."
            });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var adminId = int.Parse(userIdClaim.Value);

        var taskTitle = task.Title;
        var taskId = task.Id;

        var assignedToName =
            $"{task.AssignedTo!.FirstName} {task.AssignedTo.LastName}";

        _context.Tasks.Remove(task);

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            adminId,
            "DeletedTask",
            $"Deleted task '{taskTitle}' (ID: {taskId}) " +
            $"which was assigned to {assignedToName}."
        );

        return Ok(new
        {
            message = "Task deleted successfully."
        });
    }


}