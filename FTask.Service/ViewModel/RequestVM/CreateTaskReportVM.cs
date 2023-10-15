using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.ViewModel.RequestVM.CreateTaskReport
{
    public class TaskReportVM
    {
        public string? ReportContent { get; set; }
        [Required]
        public int TaskActivityId { get; set; }
        public IEnumerable<IFormFile> Evidences { get; set; } = new List<IFormFile>();
    }
}
