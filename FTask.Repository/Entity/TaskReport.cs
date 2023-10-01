using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class TaskReport : Auditable
{
    [Key]
    public int TaskReportId { get; set; }
    public string Content { get; set; } = "No Content";

    public TaskActivity? TaskACtivity { get; set; }
    public int? TaskACtivityId { get; set; }

    public IEnumerable<Evidence>? Evidences { get; set; }
}
