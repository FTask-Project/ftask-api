using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.IRepository;

namespace FTask.Repository.Repository
{
    internal class TaskReportRepository : BaseRepository<TaskReport, int>, ITaskReportRepository
    {
        public TaskReportRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
