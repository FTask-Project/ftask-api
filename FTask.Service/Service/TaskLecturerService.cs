using AutoMapper;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM.CreateTaskLecturer;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FTask.Service.IService
{
    internal class TaskLecturerService : ITaskLecturerService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<TaskLecturer> _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TaskLecturerService(IUnitOfWork unitOfWork, ICheckQuantityTaken checkQuantityTaken, ICacheService<TaskLecturer> cacheService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<TaskLecturer?> GetTaskLecturerById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(TaskLecturer), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var include = new Expression<Func<TaskLecturer, object>>[]
                {
                    tl => tl.Task!,
                    tl => tl.Lecturer!,
                    tl => tl.TaskActivities
                };
                var taskLecturer = await _unitOfWork.TaskLecturerRepository
                    .Get(t => !t.Deleted && t.TaskLecturerId == id, include)
                    .FirstOrDefaultAsync();
                if (taskLecturer is not null)
                {
                    await _cacheService.SetAsync(key, taskLecturer);
                }
                return taskLecturer;
            }
            return cachedData;
        }

        public async Task<IEnumerable<TaskLecturer>> GetTaskLecturers(int page, int quantity, int? taskId, Guid? lecturerId)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            var taskLecturerList = _unitOfWork.TaskLecturerRepository
                .Get(tl => !tl.Deleted)
                .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                .Take(quantity)
                .AsNoTracking();

            if (taskId is not null)
            {
                taskLecturerList = taskLecturerList.Where(tl => tl.TaskId == taskId);
            }

            if (lecturerId is not null)
            {
                taskLecturerList = taskLecturerList.Where(tl => tl.LecturerId == lecturerId);
            }

            return await taskLecturerList.ToArrayAsync();
        }

        public async Task<ServiceResponse> CreateNewTaskLecturer(CreateTaskLecturerVM newEntity)
        {
            var exsitedTask = await _unitOfWork.TaskRepository.FindAsync(newEntity.TaskId);
            if (exsitedTask is null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to assign task",
                    Errors = new string[1] {"Can not find task"}
                };
            }

            var existedLecturer = await _unitOfWork.LecturerRepository.FindAsync(newEntity.LecturerId);
            if (existedLecturer is null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to assign task",
                    Errors = new string[1] {"Can not find lecturer"}
                };
            }

            var newTaskLecturer = _mapper.Map<TaskLecturer>(newEntity);

            if (newTaskLecturer.TaskActivities.Count() > 0)
            {
                await _unitOfWork.TaskActivityRepository.AddRangeAsync(newTaskLecturer.TaskActivities);
            }

            await _unitOfWork.TaskLecturerRepository.AddAsync(newTaskLecturer);

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponse
                    {
                        Id = newTaskLecturer.TaskLecturerId.ToString(),
                        IsSuccess = true,
                        Message = "Assign task successfully"
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to assign task",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to assign task",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to assign task",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }

        public async Task<bool> DeleteTaskLecturer(int id)
        {
            var existedTaskLecturer = await _unitOfWork.TaskLecturerRepository.Get(tl => !tl.Deleted && tl.TaskLecturerId == id).FirstOrDefaultAsync();
            if(existedTaskLecturer is null)
            {
                return false;
            }
            _unitOfWork.TaskLecturerRepository.Remove(existedTaskLecturer);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(TaskLecturer), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }
    }
}
