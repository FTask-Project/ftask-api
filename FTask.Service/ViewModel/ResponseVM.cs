using FTask.Repository.Entity;
using FTask.Repository.Identity;
using Task = FTask.Repository.Entity.Task;

namespace FTask.Service.ViewModel;

public class ServiceResponse
{
    public int Id { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    //public bool? IsRestored { get; set; } = false;
}


// =============This is for login=========================
public class AuthenticateResponseVM
{
    public string? Token { get; set; }
    public UserInformationResponseVM? UserInformation { get; set; }
}
public class UserInformationResponseVM
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
}
// =======================================================


public class DepartmentResponseVM
{
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? DepartmentCode { get; set; }
    public LecturerResponseVM? DepartmentHead { get; set; }
    public IEnumerable<SubjectResponseVM> Subjects { get; set; } = Enumerable.Empty<SubjectResponseVM>();

    public IEnumerable<TaskResponseVM> Tasks { get; set; } = Enumerable.Empty<TaskResponseVM>();

    public IEnumerable<LecturerResponseVM> Lecturers { get; set; } = Enumerable.Empty<LecturerResponseVM>();
}

public class SubjectResponseVM
{
    public int SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public DepartmentResponseVM? Department { get; set; }
}

public class TaskResponseVM
{
    public int TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public string? TaskContent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TaskLevel { get; set; }
    public string? Location { get; set; }
}

public class LecturerResponseVM
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}