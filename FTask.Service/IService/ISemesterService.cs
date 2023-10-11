using FTask.Repository.Entity;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateSemester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.IService
{
    public interface ISemesterService
    {
        Task<Semester?> GetSemesterById(int id);
        Task<IEnumerable<Semester>> GetSemesters(int page, int quantity);
        Task<ServiceResponse> CreateNewSemester(SemesterVM newEntity);
    }
}
