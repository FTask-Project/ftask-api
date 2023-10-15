using FTask.Repository.Data;
using FTask.Repository.Identity;
using FTask.Repository.IRepository;

namespace FTask.Repository.Repository
{
    internal class LecturerRepository : BaseRepository<Lecturer, Guid>, ILecturerRepository
    {
        public LecturerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
