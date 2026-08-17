namespace TaskManager.Api.DTOs.Tasks;

public class TaskResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int CreatedById { get; set; }

    public string CreatedByName { get; set; } = string.Empty;

    public int AssignedToId { get; set; }

    public string AssignedToName { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Progress { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}