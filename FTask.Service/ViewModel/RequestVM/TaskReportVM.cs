using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel.RequestVM.TaskReport
{
    public class TaskReportVM
    {
        public string? ReportContent { get; set; }
        [Required]
        public int TaskActivityId { get; set; }
        public IEnumerable<IFormFile> Evidences { get; set; } = new List<IFormFile>();
        public IEnumerable<EvidenceVM> FilePaths { get; set; } = new List<EvidenceVM>();
    }

    public class UpdateTaskReportVM
    {
        public string? ReportContent { get; set; }
        public IEnumerable<IFormFile> AddEvidences { get; set; } = new List<IFormFile>();
        public IEnumerable<int> DeleteEvidences { get; set; } = new List<int>();
        public IEnumerable<EvidenceVM> AddFilePaths { get; set; } = new List<EvidenceVM>();
    }

    public class EvidenceVM
    {
        public string FileName { get; set; } = "Undefined";
        public string Url { get; set; } = "Undefined";
    }
}
