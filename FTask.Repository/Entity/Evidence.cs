using System.ComponentModel.DataAnnotations;

namespace FTask.Repository.Entity
{
    public class Evidence : Auditable
    {
        [Key]
        public int EvidenceId { get; set; }
        public string Url { get; set; } = "Undefined";

        public TaskReport? TaskReport { get; set; }
        public int TaskReportId { get; set; }

        public bool Deleted { get; set; } = false;
    }
}
