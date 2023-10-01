using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity;

public class TaskReport : Auditable
{
    [Key]
    public int TaskReportId { get; set; }
    public string ReportContent { get; set; } = "No Content";
    public int? TaskActivityId { get; set; }

    public TaskActivity? TaskActivity { get; set; }

    public IEnumerable<Evidence>? Evidences { get; set; }
}
