using FTask.Repository.Identity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FTask.Repository.Entity;

public class Department : Auditable
{
    [Key]
    public int DepartmentId { get; set; }
    [Required]
    public string DepartmentName { get; set; } = "Undefined";
    [Required]
    public string DepartmentCode { get; set; } = "Undefined";

    public Lecturer? DepartmentHead { get; set; }
    [AllowNull]
    public Guid? DepartmentHeadId { get; set; }

    public IEnumerable<Subject> Subjects { get; set; } = new List<Subject>();

    public IEnumerable<Task> Tasks { get; set; } = new List<Task>();

    public IEnumerable<Lecturer> Lecturers { get; set; } = new List<Lecturer>();

    //public bool Deleted { get; set; } = false;
}
