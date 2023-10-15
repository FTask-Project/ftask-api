using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class Semester : Auditable
{
    [Key]
    public int SemesterId { get; set; }
    [Required]
    public string SemesterCode { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public IEnumerable<Task> Tasks { get; set; } = new List<Task>();

    public bool Deleted { get; set; } = false;
}
