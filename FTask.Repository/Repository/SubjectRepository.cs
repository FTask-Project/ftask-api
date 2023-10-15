using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.IRepository;

namespace FTask.Repository.Repository
{
    internal class SubjectRepository : BaseRepository<Subject, int>, ISubjectRepository
    {
        public SubjectRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
