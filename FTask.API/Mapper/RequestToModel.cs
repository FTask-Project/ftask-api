using AutoMapper;
using FTask.Repository.Common;
using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.Department;
using FTask.Service.ViewModel.RequestVM.Semester;
using FTask.Service.ViewModel.RequestVM.Subject;
using FTask.Service.ViewModel.RequestVM.Task;
using FTask.Service.ViewModel.RequestVM.TaskActivity;
using FTask.Service.ViewModel.RequestVM.TaskLecturer;
using FTask.Service.ViewModel.RequestVM.TaskReport;
using Task = FTask.Repository.Entity.Task;

namespace FTask.API.Mapper
{
    public class RequestToModel : Profile
    {
        private readonly ICurrentUserService _currentUserService;

        public RequestToModel(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;

            CreateMap<CreateSubjectVM, Subject>();

            CreateMap<DepartmentVM, Department>()
                .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects));

            CreateMap<SubjectVM, Subject>();

            CreateMap<SemesterVM, Semester>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => _currentUserService.UserId));

            CreateMap<TaskVM, Task>()
                .ForMember(dest => dest.TaskLecturers, opt => opt.MapFrom(src => src.TaskLecturers))
                .ForMember(dest => dest.Attachments, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => _currentUserService.UserId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<TaskLecturerVM, TaskLecturer>()
                .ForMember(dest => dest.TaskActivities, opt => opt.MapFrom(src => src.TaskActivities))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => _currentUserService.UserId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<TaskActivityVM, TaskActivity>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => _currentUserService.UserId));

            CreateMap<CreateTaskLecturerVM, TaskLecturer>()
                .ForMember(dest => dest.TaskActivities, opt => opt.MapFrom(src => src.TaskActivities))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => _currentUserService.UserId));

            CreateMap<CreateTaskActivityVM, TaskActivity>();

            CreateMap<TaskReportVM, TaskReport>()
                .ForMember(dest => dest.Evidences, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => _currentUserService.UserId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
