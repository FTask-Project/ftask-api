using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    internal class TaskActivityService : ITaskActivityService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly IUnitOfWork _unitOfWork;
        public TaskActivityService(IUnitOfWork unitOfWork, ICheckQuantityTaken checkQuantityTaken)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
        }
    }
}
