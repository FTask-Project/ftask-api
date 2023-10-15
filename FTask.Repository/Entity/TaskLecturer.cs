using FTask.Repository.Identity;
using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class TaskLecturer : Auditable
{
    [Key]
    public int TaskLecturerId { get; set; }
    public string? Note { get; set; }

    public Task? Task { get; set; }
    public int TaskId { get; set; }

    public Lecturer? Lecturer { get; set; }
    public Guid LecturerId { get; set; }

    public IEnumerable<TaskActivity> TaskActivities { get; set; } = new List<TaskActivity>();

    public bool Deleted { get; set; } = false;
}
