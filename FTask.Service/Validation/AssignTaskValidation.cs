using FTask.Repository.Common;
using FTask.Service.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.Validation
{
    public interface IAssignTaskValidation
    {
        //bool CanAssignTask(IEnumerable<Guid> taskRecipients);
    }

    internal class AssignTaskValidation : IAssignTaskValidation
    {
        private static readonly string[] ALLOWED_ROLES = new string[2] { "Manager", "Admin" };

        private readonly ICurrentUserService _currentUserService;
        private readonly ILecturerService _lecturerService;

        public AssignTaskValidation(ICurrentUserService currentUserService, ILecturerService lecturerService)
        {
            _currentUserService = currentUserService;
            _lecturerService = lecturerService;
        }

        /*public async bool CanAssignTask(IEnumerable<Guid> taskRecipients)
        {
            var roles = _currentUserService.Roles;
            if(roles.Count() == 0)
            {
                return false;
            }

            if (roles.Any(r => ALLOWED_ROLES.Contains(r)))
            {
                return true;
            }

            var check = Guid.TryParse(_currentUserService.UserId, out var currentUserId);
            if (!check)
            {
                return false;
            }

            //var lecturer = await _lecturerService.GetLectureById

            return false;
        }*/
    }
}
