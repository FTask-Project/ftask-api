using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class TaskActivity : Auditable
{
    [Key]
    public int TaskActivityId { get; set; }
    public string ActivityTitle { get; set; } = "No Title";
    public string ActivityDescription { get; set; } = "No description";
    public DateTime Deadline { get; set; }

    public TaskLecturer? TaskLecturer { get; set; }
    public int TaskLecturerId { get; set; }

    public TaskReport? TaskReport { get; set; }

    public bool Deleted { get; set; } = false;
}
