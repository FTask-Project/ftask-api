using FTask.Repository.Data;
using Task = FTask.Repository.Entity.Task;

namespace FTask.Repository.IRepository
{
    public interface ITaskRepository : IBaseRepository<Task, int>
    {
    }
}
