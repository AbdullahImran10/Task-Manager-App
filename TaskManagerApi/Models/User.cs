
namespace TaskManager.Api.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
      public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool isActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLogin { get; set; }
    public Role? Role { get; set; }
    public ICollection<TaskItem> CreatedTasks { get; set; }
    = new List<TaskItem>();

    public ICollection<TaskItem> AssignedTasks { get; set; }
    = new List<TaskItem>();

    public ICollection<Notification> Notifications { get; set; }
    = new List<Notification>();

    public ICollection<AuditLog> AuditLogs { get; set; }
    = new List<AuditLog>();
}