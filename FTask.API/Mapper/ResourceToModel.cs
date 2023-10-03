using AutoMapper;
using FTask.Repository.Entity;
using FTask.Repository.Identity;
using FTask.Service.ViewModel;
using Task = FTask.Repository.Entity.Task;

namespace FTask.API.Mapper
{
    public class ResourceToModel : Profile
    {
        public ResourceToModel()
        {
            CreateMap<User, UserInformationResponseVM>();

            CreateMap<Lecturer, UserInformationResponseVM>();

            CreateMap<Lecturer, LecturerResponseVM>();

            CreateMap<Subject, SubjectResponseVM>();

            CreateMap<Task, TaskResponseVM>();

            CreateMap<Department, DepartmentResponseVM>()
                .ForMember(dest => dest.DepartmentHead, opt => opt.MapFrom(src => src.DepartmentHead))
                .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects))
                .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks))
                .ForMember(dest => dest.Lecturers, opt => opt.MapFrom(src => src.Lecturers));
        }
    }
}
