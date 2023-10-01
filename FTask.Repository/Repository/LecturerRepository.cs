using FTask.Repository.Data;
using FTask.Repository.Identity;
using FTask.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Repository.Repository
{
    internal class LecturerRepository : BaseRepository<Lecturer,Guid>, ILecturerRepository
    {
        public LecturerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
