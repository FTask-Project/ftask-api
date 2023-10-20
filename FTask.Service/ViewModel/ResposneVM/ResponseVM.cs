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


// ======================================
public class AuthenticateResponseVM
{
    public string? Token { get; set; }
    public UserInformationResponseVM? UserInformation { get; set; }
    public LecturerInformationResponseVM? LecturerInformation { get; set; }
}
public class UserInformationResponseVM : Auditable
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
    public IEnumerable<RoleResponseVM> Roles { get; set; } = new List<RoleResponseVM>();
}

public class LecturerInformationResponseVM : Auditable
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

    public DepartmentResponseVM? Department { get; set; }
    public DepartmentResponseVM? DepartmentHead { get; set; }
    public IEnumerable<SubjectResponseVM> Subjects { get; set; } = new List<SubjectResponseVM>();
}
// =======================================================

public class RoleResponseVM : Auditable
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}


public class DepartmentResponseVM : Auditable
{
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? DepartmentCode { get; set; }
    public LecturerResponseVM? DepartmentHead { get; set; }
    public IEnumerable<SubjectResponseVM> Subjects { get; set; } = new List<SubjectResponseVM>();

    public IEnumerable<TaskResponseVM> Tasks { get; set; } = new List<TaskResponseVM>();

    public IEnumerable<LecturerResponseVM> Lecturers { get; set; } = new List<LecturerResponseVM>();
}

public class SubjectResponseVM : Auditable
{
    public int SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public DepartmentResponseVM? Department { get; set; }
}

public class TaskResponseVM : Auditable
{
    public int TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public string? TaskContent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TaskLevel { get; set; }
    public int TaskStatus { get; set; }
    public string? Location { get; set; }
    public SemesterResponseVM? Semester { get; set; }
    public DepartmentResponseVM? Department { get; set; }
    public SubjectResponseVM? Subject { get; set; }
    public IEnumerable<TaskLecturerResponseVM> TaskLecturers { get; set; } = new List<TaskLecturerResponseVM>();
    public IEnumerable<AttachmentResponseVM> Attachments { get; set; } = new List<AttachmentResponseVM>();
}

public class AttachmentResponseVM
{
    public int AttachmentId { get; set; }
    public string? Url { get; set; }
    public string? FileName { get; set; }
}

public class TaskLecturerResponseVM : Auditable
{
    public int TaskLecturerId { get; set; }
    public string? Note { get; set; }
    public TaskResponseVM? Task { get; set; }
    public LecturerResponseVM? Lecturer { get; set; }
    public IEnumerable<TaskActivityResponseVM> TaskActivities { get; set; } = new List<TaskActivityResponseVM>();
}

public class TaskActivityResponseVM : Auditable
{
    public int TaskActivityId { get; set; }
    public string? ActivityTitle { get; set; }
    public string? ActivityDescription { get; set; }
    public DateTime Deadline { get; set; }
    public int TaskActivityStatus { get; set; }
    public TaskLecturerResponseVM? TaskLecturer { get; set; }
    public TaskReportResponseVM? TaskReport { get; set; }
}

public class TaskReportResponseVM : Auditable
{
    public int TaskReportId { get; set; }
    public string? ReportContent { get; set; }
    public TaskActivityResponseVM? TaskActivity { get; set; }
    public IEnumerable<EvidenceResponseVM> Evidences { get; set; } = new List<EvidenceResponseVM>();
}

public class EvidenceResponseVM : Auditable
{
    public int EvidenceId { get; set; }
    public string? Url { get; set; }
    public string? FileName { get; set; }
}

public class SemesterResponseVM : Auditable
{
    public int SemesterId { get; set; }
    public string? SemesterCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}


public class LecturerResponseVM : Auditable
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


public abstract class Auditable
{
    public string CreatedBy { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }
}