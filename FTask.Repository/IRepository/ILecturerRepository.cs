using FTask.Repository.Data;
using FTask.Repository.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Repository.IRepository
{
    public interface ILecturerRepository : IBaseRepository<Lecturer,Guid>
    {
    }
}
