using FTask.Repository.Identity;
using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class TaskLecturer : Auditable
{
    [Key]
    public int TaskUserId { get; set; }
    public string? Note { get; set; }

    public Task? Task { get; set; }
    public int TaskId { get; set; }

    public Lecturer? User { get; set; }
    public Guid UserId { get; set; }

    //public Subject? Subject { get; set; }
    //public int SubjectId { get; set; }

    public IEnumerable<TaskActivity>? TaskActivities { get; set; }
}
