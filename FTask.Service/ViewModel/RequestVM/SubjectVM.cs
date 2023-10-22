using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel.RequestVM.Subject
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

    public class UpdateSubjectVM
    {
        public string? SubjectName { get; set; }
        public string? SubjectCode { get; set; }
        public bool? Status { get; set; }
        public int? DepartmentId { get; set; }
    }
}
