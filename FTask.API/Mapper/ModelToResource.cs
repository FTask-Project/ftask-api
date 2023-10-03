using AutoMapper;
using FTask.Repository.Entity;
using FTask.Service.ViewModel;

namespace FTask.API.Mapper;

public class ModelToResource : Profile
{
    public ModelToResource()
    {
        CreateMap<SubjectVM, Subject>();

        CreateMap<DepartmentVM, Department>()
            .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects));
    }
}
