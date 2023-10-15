using FTask.Repository.Data;

namespace FTask.Service.IService
{
    internal class AttachmentService : IAttachmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AttachmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
