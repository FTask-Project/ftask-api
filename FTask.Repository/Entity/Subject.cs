using FTask.Repository.Identity;
using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class Subject : Auditable
{
    [Key]
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = "Undefined";
    [Required]
    public string SubjectCode { get; set; } = null!;
    public bool Status { get; set; }

    public Department? Department { get; set; }
    public int DepartmentId { get; set; }

    public IEnumerable<Lecturer> Lecturers { get; set; } = new List<Lecturer>();

    public IEnumerable<Task> Tasks { get; set; } = new List<Task>();

    public bool Deleted { get; set; } = false; 
}
