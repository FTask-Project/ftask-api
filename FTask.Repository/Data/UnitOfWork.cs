using FTask.Repository.IRepository;

namespace FTask.Repository.Data;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IRoleRepository RoleRepository { get; }
    ILecturerRepository LecturerRepository { get; }
    IAttachmentRepository AttachmentRepository { get; }
    IDepartmentRepository DepartmentRepository { get; }
    IEvidenceRepository EvidenceRepository { get; }
    ISemesterRepository SemesterRepository { get; }
    ISubjectRepository SubjectRepository { get; }
    ITaskActivityRepository TaskActivityRepository { get; }
    ITaskLecturerRepository TaskLecturerRepository { get; }
    ITaskReportRepository TaskReportRepository { get; }
    ITaskRepository TaskRepository { get; }

    Task<bool> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _applicationDbContext;

    public IUserRepository UserRepository { get; private set; }
    public IRoleRepository RoleRepository { get; private set; }
    public ILecturerRepository LecturerRepository { get; private set; }
    public IAttachmentRepository AttachmentRepository { get; private set; }
    public IDepartmentRepository DepartmentRepository { get; private set; }
    public IEvidenceRepository EvidenceRepository { get; private set; }
    public ISemesterRepository SemesterRepository { get; private set; }
    public ISubjectRepository SubjectRepository { get; private set; }
    public ITaskActivityRepository TaskActivityRepository { get; private set; }
    public ITaskLecturerRepository TaskLecturerRepository { get; private set; }
    public ITaskReportRepository TaskReportRepository { get; private set; }
    public ITaskRepository TaskRepository { get; private set; }

    public UnitOfWork(ApplicationDbContext applicationDbContext,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ILecturerRepository lecturerRepository,
        IAttachmentRepository attachmentRepository,
        IDepartmentRepository departmentRepository,
        IEvidenceRepository evidenceRepository,
        ISemesterRepository semesterRepository,
        ISubjectRepository subjectRepository,
        ITaskActivityRepository taskActivityRepository,
        ITaskLecturerRepository taskLecturerRepository,
        ITaskReportRepository taskReportRepository,
        ITaskRepository taskRepository)
    {
        _applicationDbContext = applicationDbContext;
        UserRepository = userRepository;
        RoleRepository = roleRepository;
        LecturerRepository = lecturerRepository;
        AttachmentRepository = attachmentRepository;
        DepartmentRepository = departmentRepository;
        EvidenceRepository = evidenceRepository;
        SemesterRepository = semesterRepository;
        SubjectRepository = subjectRepository;
        TaskActivityRepository = taskActivityRepository;
        TaskLecturerRepository = taskLecturerRepository;
        TaskReportRepository = taskReportRepository;
        TaskRepository = taskRepository;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return (await _applicationDbContext.SaveChangesAsync()) > 0;
    }

    public void Dispose()
    {
        _applicationDbContext.Dispose();
    }
}
