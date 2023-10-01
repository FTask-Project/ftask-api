using AutoMapper;
using FTask.Repository.Identity;
using FTask.Service.ViewModel;

namespace FTask.API.Mapper
{
    public class ResourceToModel : Profile
    {
        public ResourceToModel()
        {
            CreateMap<User, UserInformationResponseVM>();
            CreateMap<Lecturer, UserInformationResponseVM>();
        }
    }
}
