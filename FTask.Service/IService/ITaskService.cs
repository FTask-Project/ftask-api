using FTask.Service.ViewModel.RequestVM;
using FTask.Service.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = FTask.Repository.Entity.Task;
using FTask.Service.ViewModel.RequestVM.CreateTask;

namespace FTask.Service.IService
{
    public interface ITaskService
    {
        Task<Task?> GetTaskById(int id);
        Task<IEnumerable<Task>> GetTasks(int page, int quantity, string filter, int? semsesterId, int? departmentId, int? subjectId, int? status);
        Task<ServiceResponse> CreateNewTask(TaskVM newEntity);
    }
}
