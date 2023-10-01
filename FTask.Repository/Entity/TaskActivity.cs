using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class TaskActivity : Auditable
{
    [Key]
    public int TaskActivityId { get; set; }
    public string Title { get; set; } = "No Title";
    public string Description { get; set; } = "No description";
    public DateTime Deadline { get; set; }

    public TaskLecturer? TaskUser { get; set; }
    public int TaskUserId { get; set; }

    public TaskReport? TaskReport { get; set; }
}
