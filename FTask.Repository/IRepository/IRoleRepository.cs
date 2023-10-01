using FTask.Repository.Data;
using FTask.Repository.Identity;

namespace FTask.Repository.IRepository;

public interface IRoleRepository : IBaseRepository<Role, Guid>
{
}
