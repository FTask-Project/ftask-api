using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.IRepository;

namespace FTask.Repository.Repository
{
    internal class SemesterRepository : BaseRepository<Semester, int>, ISemesterRepository
    {
        public SemesterRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
