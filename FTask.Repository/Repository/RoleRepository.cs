using FTask.Repository.Data;
using FTask.Repository.Identity;
using FTask.Repository.IRepository;

namespace FTask.Repository.Repository;

internal class RoleRepository : BaseRepository<Role, Guid>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
