using AutoMapper;
using FTask.Repository.Entity;
using FTask.Repository.Identity;
using FTask.Service.ViewModel.ResposneVM;
using Task = FTask.Repository.Entity.Task;

namespace FTask.API.Mapper;

public class ModelToResponse : Profile
{
    public ModelToResponse()
    {
        CreateMap<ServiceResponse<Department>, ServiceResponseVM>();
        CreateMap<ServiceResponse<Lecturer>, ServiceResponseVM>();
        CreateMap<ServiceResponse<Role>, ServiceResponseVM>();
        CreateMap<ServiceResponse<Semester>, ServiceResponseVM>();
        CreateMap<ServiceResponse<Subject>, ServiceResponseVM>();
        CreateMap<ServiceResponse<TaskActivity>, ServiceResponseVM>();
        CreateMap<ServiceResponse<Task>, ServiceResponseVM>();
        CreateMap<ServiceResponse<TaskLecturer>, ServiceResponseVM>();
        CreateMap<ServiceResponse<TaskReport>, ServiceResponseVM>();
        CreateMap<ServiceResponse<User>, ServiceResponseVM>();

        CreateMap<Role, RoleResponseVM>();

        CreateMap<User, UserInformationResponseVM>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Where(r => !r.Deleted)));

        CreateMap<Lecturer, LecturerInformationResponseVM>()
            .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects.Where(s => !s.Deleted)));

        CreateMap<Lecturer, LecturerResponseVM>();




        CreateMap<Task, TaskResponseVM>()
            .ForMember(dest => dest.TaskLecturers, opt => opt.MapFrom(src => src.TaskLecturers.Where(tl => !tl.Deleted)))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments.Where(a => !a.Deleted)));

        CreateMap<TaskLecturer, TaskLecturerResponseVM>()
            .ForMember(dest => dest.TaskActivities, opt => opt.MapFrom(src => src.TaskActivities.Where(ta => !ta.Deleted)));

        CreateMap<TaskActivity, TaskActivityResponseVM>();

        CreateMap<TaskReport, TaskReportResponseVM>()
            .ForMember(dest => dest.Evidences, opt => opt.MapFrom(src => src.Evidences.Where(e => !e.Deleted)));

        CreateMap<Evidence, EvidenceResponseVM>();

        CreateMap<Attachment, AttachmentResponseVM>();




        CreateMap<Semester, SemesterResponseVM>();

        CreateMap<Department, DepartmentResponseVM>()
            .ForMember(dest => dest.DepartmentHead, opt => opt.MapFrom(src => src.DepartmentHead!.Deleted ? null : src.DepartmentHead))
            .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects.Where(s => !s.Deleted)))
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks.Where(t => !t.Deleted)))
            .ForMember(dest => dest.Lecturers, opt => opt.MapFrom(src => src.Lecturers.Where(l => !l.Deleted)));

        CreateMap<Subject, SubjectResponseVM>();




        // Statistics
        CreateMap<Task, TaskStatisticResponseVM>();
    }
}
