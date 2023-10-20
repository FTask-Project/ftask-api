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
        CreateMap<ServiceResponse, ServiceResponseVM>();

        CreateMap<Role, RoleResponseVM>();

        CreateMap<User, UserInformationResponseVM>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles));

        CreateMap<Lecturer, LecturerInformationResponseVM>()
            .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects));

        CreateMap<Lecturer, LecturerResponseVM>();

        CreateMap<Subject, SubjectResponseVM>();

        CreateMap<Task, TaskResponseVM>()
            .ForMember(dest => dest.TaskLecturers, opt => opt.MapFrom(src => src.TaskLecturers))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

        CreateMap<Department, DepartmentResponseVM>()
            .ForMember(dest => dest.DepartmentHead, opt => opt.MapFrom(src => src.DepartmentHead))
            .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects))
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks))
            .ForMember(dest => dest.Lecturers, opt => opt.MapFrom(src => src.Lecturers));

        CreateMap<Semester, SemesterResponseVM>();

        CreateMap<TaskLecturer, TaskLecturerResponseVM>()
            .ForMember(dest => dest.TaskActivities, opt => opt.MapFrom(src => src.TaskActivities));

        CreateMap<TaskActivity, TaskActivityResponseVM>();

        CreateMap<TaskReport, TaskReportResponseVM>()
            .ForMember(dest => dest.Evidences, opt => opt.MapFrom(src => src.Evidences));

        CreateMap<Evidence, EvidenceResponseVM>();

        CreateMap<Attachment, AttachmentResponseVM>();
    }
}
