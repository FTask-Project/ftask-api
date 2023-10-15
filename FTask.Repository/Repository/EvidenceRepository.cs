using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.IRepository;

namespace FTask.Repository.Repository
{
    internal class EvidenceRepository : BaseRepository<Evidence, int>, IEvidenceRepository
    {
        public EvidenceRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
