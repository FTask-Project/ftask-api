using FTask.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    internal class TaskLecturerService : ITaskLecturerService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TaskLecturerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
