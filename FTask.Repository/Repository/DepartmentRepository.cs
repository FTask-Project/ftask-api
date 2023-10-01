using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Repository.Repository
{
    internal class DepartmentRepository : BaseRepository<Department,int>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
