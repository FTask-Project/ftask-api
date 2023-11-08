using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public int TaskStatus { get; set; } = 1;

    public IEnumerable<TaskLecturer> TaskLecturers { get; set; } = new List<TaskLecturer>();

    public IEnumerable<Attachment> Attachments { get; set; } = new List<Attachment>();

    public Subject? Subject { get; set; }
    public int? SubjectId { get; set; }

    public Semester? Semester { get; set; }
    public int SemesterId { get; set; }

    public Department? Department { get; set; }
    public int? DepartmentId { get; set; }

    //public bool Deleted { get; set; } = false;

    [NotMapped]
    public Creator? Creator { get; set; }
}

public class Creator
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? FilePath { get; set; }

    public Creator(string? name, string? email, string? filePath)
    {
        Name = name;
        Email = email;
        FilePath = filePath;
    }
}
