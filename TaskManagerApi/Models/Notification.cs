namespace TaskManager.Api.Models;
public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = "Info";

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional link to a task
    public int? TaskId { get; set; }

    public User? User { get; set; }

    public TaskItem? Task { get; set; }
}