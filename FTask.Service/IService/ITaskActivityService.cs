using FTask.Repository.Entity;
using FTask.Service.ViewModel.RequestVM.CreateTaskActivity;
using FTask.Service.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    public interface ITaskActivityService
    {
        Task<TaskActivity?> GetTaskActivityById(int id);
        Task<IEnumerable<TaskActivity>> GetTaskActivities(int page, int quantity, string filter, int? taskLecturerId);
        Task<ServiceResponse> CreateNewActivity(CreateTaskActivityVM newEntity);
    }
}
