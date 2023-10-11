using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.ViewModel.RequestVM.CreateDepartment;

public class DepartmentVM
{
    [Required]
    public string? DepartmentName { get; set; }
    [Required]
    public string? DepartmentCode { get; set; }
    public Guid? DepartmentHeadId { get; set; }
    public IEnumerable<SubjectVM> Subjects { get; set; } = Enumerable.Empty<SubjectVM>();
}

public class SubjectVM
{
    public string SubjectName { get; set; } = "Undefined";
    [Required]
    public string? SubjectCode { get; set; }
    public bool Status { get; set; }
}
