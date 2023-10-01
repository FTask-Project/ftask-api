using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Repository.Entity
{
    public class Evidence : Auditable
    {
        [Key]
        public int EvidenceId { get; set; }
        public string Url { get; set; } = "Undefined";
        public int TaskReportId { get; set; }

        public TaskReport? TaskReport { get; set; }
    }
}
