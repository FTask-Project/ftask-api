using AutoMapper;
using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;
using CreateTaskActivityVM = FTask.Service.ViewModel.RequestVM.CreateTaskActivity.CreateTaskActivityVM;

namespace FTask.Service.IService
{
    internal class TaskActivityService : ITaskActivityService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<TaskActivity> _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public TaskActivityService(IUnitOfWork unitOfWork, ICheckQuantityTaken checkQuantityTaken, ICacheService<TaskActivity> cacheService, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<TaskActivity?> GetTaskActivityById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(TaskActivity), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var taskActivity = await _unitOfWork.TaskActivityRepository.Get(a => !a.Deleted && a.TaskActivityId == id).FirstOrDefaultAsync();
                if (taskActivity is not null)
                {
                    await _cacheService.SetAsync(key, taskActivity);
                }
                return taskActivity;
            }

            return cachedData;
        }

        public async Task<IEnumerable<TaskActivity>> GetTaskActivities(int page, int quantity, string filter, int? taskLecturerId)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            var taskActivityList = _unitOfWork.TaskActivityRepository
                .Get(a => !a.Deleted && a.ActivityTitle.Contains(filter))
                .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                .Take(quantity)
                .AsNoTracking();

            if (taskLecturerId is not null)
            {
                taskActivityList = taskActivityList.Where(a => a.TaskLecturerId == taskLecturerId);
            }

            return await taskActivityList.ToArrayAsync();
        }

        public async Task<ServiceResponse> CreateNewActivity(CreateTaskActivityVM newEntity)
        {
            var existedTaskLecturer = await _unitOfWork.TaskLecturerRepository.FindAsync(newEntity.TaskLecturerId);
            if (existedTaskLecturer is null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new task activity",
                    Errors = new List<string>() { "Can not find assigned task" }
                };
            }

            var newTaskActivity = _mapper.Map<TaskActivity>(newEntity);

            await _unitOfWork.TaskActivityRepository.AddAsync(newTaskActivity);

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponse
                    {
                        Id = newTaskActivity.TaskActivityId.ToString(),
                        IsSuccess = true,
                        Message = "Create new task activity successfully"
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new task activity",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new task",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<bool> DeleteTaskActivity(int id)
        {
            var existedTaskActivity = await _unitOfWork.TaskActivityRepository.Get(a => !a.Deleted && a.TaskActivityId == id).FirstOrDefaultAsync();
            if(existedTaskActivity is null)
            {
                return false;
            }
            _unitOfWork.TaskActivityRepository.Remove(existedTaskActivity);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(TaskActivity), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }
    }
}
