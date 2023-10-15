using FTask.Repository.Data;

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
