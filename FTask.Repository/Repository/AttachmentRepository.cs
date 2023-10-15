using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.IRepository;

namespace FTask.Repository.Repository
{
    internal class AttachmentRepository : BaseRepository<Attachment, int>, IAttachmentRepository
    {
        public AttachmentRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
