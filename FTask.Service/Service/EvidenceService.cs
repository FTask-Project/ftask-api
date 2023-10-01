using FTask.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    internal class EvidenceService : IEvidenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        public EvidenceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
