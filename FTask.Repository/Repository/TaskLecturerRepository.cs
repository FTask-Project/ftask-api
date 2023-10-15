using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.IRepository;

namespace FTask.Repository.Repository
{
    internal class TaskLecturerRepository : BaseRepository<TaskLecturer, int>, ITaskLecturerRepository
    {
        public TaskLecturerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
