namespace FTask.Service.ViewModel.ResposneVM;

public class ServiceResponse
{
    public string? Id { get; set; }
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    //public bool? IsRestored { get; set; } = false;
}

public class ServiceResponseVM
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string>? Errors { get; set; }
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
    public string? FilePath { get; set; }
    public string? DisplayName { get; set; }
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
    public IEnumerable<TaskLecturerResponseVM> TaskLecturers { get; set; } = new List<TaskLecturerResponseVM>();
}

public class TaskLecturerResponseVM
{
    public int TaskLecturerId { get; set; }
    public string? Note { get; set; }
    public TaskResponseVM? Task { get; set; }
    public LecturerResponseVM? Lecturer { get; set; }
}

public class TaskActivityResponseVM
{
    public int TaskActivityId { get; set; }
    public string? ActivityTitle { get; set; }
    public string? ActivityDescription { get; set; }
    public DateTime Deadline { get; set; }
    public TaskLecturerResponseVM? TaskLecturer { get; set; }
}

public class TaskReportResponseVM
{
    public int TaskReportId { get; set; }
    public string? ReportContent { get; set; }
    public TaskActivityResponseVM? TaskActivity { get; set; }
    public IEnumerable<string> Evidences { get; set; } = new List<string>();
}

public class LecturerResponseVM
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

public class RoleResponseVM
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}

public class SemesterResponseVM
{
    public int SemesterId { get; set; }
    public string? SemesterCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}