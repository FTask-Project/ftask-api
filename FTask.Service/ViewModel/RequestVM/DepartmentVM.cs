using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel.RequestVM.Department;

public class DepartmentVM
{
    [Required]
    public string DepartmentName { get; set; } = null!;
    [Required]
    public string DepartmentCode { get; set; } = null!;
    public Guid? DepartmentHeadId { get; set; }
    public IEnumerable<SubjectVM> Subjects { get; set; } = new List<SubjectVM>();
}

public class SubjectVM
{
    public string SubjectName { get; set; } = "Undefined";
    [Required]
    public string? SubjectCode { get; set; }
    public bool Status { get; set; }
}

public class UpdateDepartmentVM
{
    public string? DepartmentName { get; set; }
    public string? DepartmentCode { get; set; }
    public Guid? DepartmentHeadId { get; set; }
}
