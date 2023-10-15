using AutoMapper;
using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.CreateDepartment;
using FTask.Service.ViewModel.RequestVM.CreateSubject;
using FTask.Service.ViewModel.RequestVM.CreateTask;
using FTask.Service.ViewModel.RequestVM.CreateTaskActivity;
using FTask.Service.ViewModel.RequestVM.CreateTaskLecturer;
using FTask.Service.ViewModel.RequestVM.CreateTaskReport;
using Task = FTask.Repository.Entity.Task;

namespace FTask.API.Mapper
{
    public class ResourceToModel : Profile
    {
        public ResourceToModel()
        {
            CreateMap<CreateSubjectVM, Subject>();

            CreateMap<DepartmentVM, Department>()
                .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects));

            CreateMap<TaskVM, Task>()
                .ForMember(dest => dest.TaskLecturers, opt => opt.MapFrom(src => src.TaskLecturers))
                .ForMember(dest => dest.Attachments, opt => opt.Ignore());

            CreateMap<TaskLecturerVM, TaskLecturer>()
                .ForMember(dest => dest.TaskActivities, opt => opt.MapFrom(src => src.TaskActivities));

            CreateMap<TaskActivityVM, TaskActivity>();

            CreateMap<CreateTaskLecturerVM, TaskLecturer>()
                .ForMember(dest => dest.TaskActivities, opt => opt.MapFrom(src => src.TaskActivities));

            CreateMap<CreateTaskActivityVM, TaskActivity>();

            CreateMap<TaskReportVM, TaskReport>()
                .ForMember(dest => dest.Evidences, opt => opt.Ignore());
        }
    }
}
