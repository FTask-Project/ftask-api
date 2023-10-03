using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel;

public class LoginUserVM
{
    [Required]
    public string UserName { get; set; } = "";

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "";

    public bool IsLecturer { get; set; } = false;

    public bool IsMember { get; set; } = false;
}

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