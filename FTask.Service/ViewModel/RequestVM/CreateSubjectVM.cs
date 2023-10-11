using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.ViewModel.RequestVM.CreateSubject
{
    public class CreateSubjectVM
    {
        [Required]
        public int DepartmentId { get; set; }
        public string SubjectName { get; set; } = "Undefined";
        [Required]
        public string? SubjectCode { get; set; }
        public bool Status { get; set; }
    }
}
