using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

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

public class LecturerVM
{
    [Required]
    public string UserName { get; set; } = "Undefined";
    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "Undefined";
    public string? PhoneNumber { get; set; }
    public bool LockoutEnabled { get; set; } = false;
    public DateTimeOffset? LockoutEnd { get; set; }
    [EmailAddress]
    [AllowNull]
    public string? Email { get; set; }
    public int? DepartmentId { get; set; }
    public IEnumerable<int> SubjectIds { get; set; } = Enumerable.Empty<int>();
}