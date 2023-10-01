using FTask.Repository.Identity;
using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class Department : Auditable
{
    [Key]
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "Undefined";
    [Required]
    public string? Code { get; set; }

    public Lecturer? Manager { get; set; }
    public Guid ManagerId { get; set; }

    public IEnumerable<Subject>? Subjects { get; set; }

    public IEnumerable<Task>? Tasks { get; set; }

    public IEnumerable<Lecturer>? Lecturers { get; set; }
}
