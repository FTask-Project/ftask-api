using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.IRepository;

namespace FTask.Repository.Repository
{
    internal class TaskActivityRepository : BaseRepository<TaskActivity, int>, ITaskActivityRepository
    {
        public TaskActivityRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
