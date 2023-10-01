using FTask.Repository.Data;
using FTask.Repository.Identity;

namespace FTask.Repository.IRepository;

public interface IUserRepository : IBaseRepository<User, Guid>
{
}
