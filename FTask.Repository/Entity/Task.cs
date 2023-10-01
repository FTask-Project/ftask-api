using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class Task : Auditable
{
    [Key]
    public int TaskId { get; set; }
    public string Title { get; set; } = "No Title";
    public string? Content { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Level { get; set; }
    public string? Location { get; set; }

    public IEnumerable<TaskLecturer>? TaskUsers { get; set; }

    public IEnumerable<Attachment>? Attachments { get; set; }

    public Subject? Subject { get; set; }
    public int SubjectId { get; set; }

    public Semester? Semester { get; set; }
    public int SemesterId { get; set; }

    public Department? Department { get; set; }
    public int DepartmentId { get; set; }
}
