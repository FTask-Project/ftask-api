using FTask.Repository.Identity;
using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class Department : Auditable
{
    [Key]
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "Undefined";
    [Required]
    public string? DepartmentCode { get; set; }
    public Guid DepartmentHeadId { get; set; }

    public Lecturer? DepartmentHead { get; set; }

    public IEnumerable<Subject>? Subjects { get; set; }

    public IEnumerable<Task>? Tasks { get; set; }

    public IEnumerable<Lecturer>? Lecturers { get; set; }
}
