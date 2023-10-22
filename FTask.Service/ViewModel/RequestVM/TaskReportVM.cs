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
}
