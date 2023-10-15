using FTask.Repository.Entity;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateTaskLecturer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    public interface ITaskLecturerService
    {
        Task<TaskLecturer?> GetTaskLecturerById(int id);
        Task<ServiceResponse> CreateNewTaskLecturer(CreateTaskLecturerVM newEntity);
        Task<IEnumerable<TaskLecturer>> GetTaskLecturers(int page, int quantity, int? taskId, Guid? lecturerId);
    }
}
