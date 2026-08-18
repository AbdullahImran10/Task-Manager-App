
namespace TaskManager.Api.Models;
public class TaskItem
{
     public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int CreatedById { get; set; }

    public int AssignedToId { get; set; }

    public string Priority { get; set; } = "Medium";

    public string Status { get; set; } = "Pending";

    public int Progress { get; set; } = 0;

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public User? CreatedBy { get; set; }

    public User? AssignedTo { get; set; }
    public ICollection<Notification> Notifications { get; set; }
    = new List<Notification>();
}