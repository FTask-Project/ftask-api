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
    }

    public class UpdateTaskReportVM
    {
        public string? ReportContent { get; set; }
        public IEnumerable<IFormFile> AddEvidences { get; set; } = new List<IFormFile>();
        public IEnumerable<int> DeleteEvidences { get; set; } = new List<int>();
    }
}
