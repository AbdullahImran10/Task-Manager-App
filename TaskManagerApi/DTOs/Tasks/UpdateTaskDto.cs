namespace TaskManager.Api.DTOs.Tasks;

public class UpdateTaskDto
{
     public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int? AssignedToId { get; set; }

    public string Priority { get; set; } = "Medium";

    public string Status { get; set; } = "Pending";

    public int Progress { get; set; }

    public DateTime? DueDate { get; set; }
}