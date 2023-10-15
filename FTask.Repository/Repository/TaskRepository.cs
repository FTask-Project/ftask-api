using FTask.Repository.Data;
using FTask.Repository.IRepository;
using Task = FTask.Repository.Entity.Task;

namespace FTask.Repository.Repository
{
    internal class TaskRepository : BaseRepository<Task, int>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
