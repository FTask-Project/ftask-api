using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class Task : Auditable
{
    [Key]
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = "No Title";
    public string? TaskContent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TaskLevel { get; set; } = 1;
    public string? Location { get; set; }
    public int TaskStatus { get; set; }

    public IEnumerable<TaskLecturer> TaskLecturers { get; set; } = new List<TaskLecturer>();

    public IEnumerable<Attachment> Attachments { get; set; } = new List<Attachment>();

    public Subject? Subject { get; set; }
    public int? SubjectId { get; set; }

    public Semester? Semester { get; set; }
    public int SemesterId { get; set; }

    public Department? Department { get; set; }
    public int? DepartmentId { get; set; }

    public bool Deleted { get; set; } = false;
}
