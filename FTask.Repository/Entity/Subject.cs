using FTask.Repository.Identity;
using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class Subject : Auditable
{
    [Key]
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = "Undefined";
    [Required]
    public string? SubjectCode { get; set; }
    public bool Status { get; set; }
    public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public IEnumerable<Lecturer>? Lecturers { get; set; }

    public IEnumerable<Task>? Tasks { get; set; }
}
