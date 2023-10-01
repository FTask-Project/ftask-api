using FTask.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = FTask.Repository.Entity.Task;

namespace FTask.Repository.IRepository
{
    public interface ITaskRepository : IBaseRepository<Task,int>
    {
    }
}
