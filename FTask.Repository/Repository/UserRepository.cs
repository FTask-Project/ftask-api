using FTask.Repository.Data;
using FTask.Repository.Identity;
using FTask.Repository.IRepository;

namespace FTask.Repository.Repository;

internal class UserRepository : BaseRepository<User, Guid>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
