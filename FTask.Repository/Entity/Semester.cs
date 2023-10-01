using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class Semester : Auditable
{
    [Key]
    public int SemesterId { get; set; }
    [Required]
    public string? Code { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public IEnumerable<Task>? Tasks { get; set; }
}
