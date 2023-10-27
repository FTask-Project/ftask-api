using AutoMapper;
using CloudinaryDotNet;
using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM.TaskActivity;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
                var taskActivity = await _unitOfWork.TaskActivityRepository
                    .Get(a => !a.Deleted && a.TaskActivityId == id)
                    .Include(nameof(TaskActivity.TaskReport) + "." + nameof(TaskReport.Evidences))
                    .Include(nameof(TaskActivity.TaskLecturer) + "." + nameof(TaskLecturer.Task))
                    .FirstOrDefaultAsync();
                if (taskActivity is not null)
                {
                    await _cacheService.SetAsync(key, taskActivity);
                }
                return taskActivity;
            }

            return cachedData;
        }

        public async Task<IEnumerable<TaskActivity>> GetTaskActivities(int page, int quantity, string filter, int? taskLecturerId, Guid? lecturerId, DateTime? from, DateTime? to)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            var includes = new Expression<Func<TaskActivity, object>>[]
            {
                ta => ta.TaskLecturer!
            };

            var taskActivityList = _unitOfWork.TaskActivityRepository
                .Get(a => !a.Deleted && a.ActivityTitle.Contains(filter), includes)
                .AsNoTracking();

            if (taskLecturerId is not null)
            {
                taskActivityList = taskActivityList.Where(a => a.TaskLecturerId == taskLecturerId);
            }

            if(lecturerId is not null)
            {
                taskActivityList = taskActivityList.Where(ta => ta.TaskLecturer!.LecturerId == lecturerId);
            }

            if(from is not null)
            {
                var fromValue = from.Value;
                from = new DateTime(fromValue.Year, fromValue.Month, fromValue.Day, 0, 0, 0);
                taskActivityList = taskActivityList.Where(ta => ta.Deadline >= from);
            }

            if(to is not null)
            {
                var toValue = to.Value;
                to = new DateTime(toValue.Year, toValue.Month, toValue.Day, 23, 59, 59);
                taskActivityList = taskActivityList.Where(ta => ta.Deadline <= to);
            }

            return await taskActivityList
                .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                .Take(quantity)
                .ToArrayAsync();
        }

        public async Task<ServiceResponse<TaskActivity>> CreateNewActivity(CreateTaskActivityVM newEntity)
        {
            var existedTaskLecturer = await _unitOfWork.TaskLecturerRepository.FindAsync(newEntity.TaskLecturerId);
            if (existedTaskLecturer is null)
            {
                return new ServiceResponse<TaskActivity>
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
                    string key = CacheKeyGenerator.GetKeyById(nameof(TaskLecturer), existedTaskLecturer.TaskLecturerId.ToString());
                    var task = _cacheService.RemoveAsync(key);

                    return new ServiceResponse<TaskActivity>
                    {
                        Entity = newTaskActivity,
                        IsSuccess = true,
                        Message = "Create new task activity successfully"
                    };
                }
                else
                {
                    return new ServiceResponse<TaskActivity>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new task activity",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<TaskActivity>
                {
                    IsSuccess = false,
                    Message = "Failed to create new task activity",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<TaskActivity>
                {
                    IsSuccess = false,
                    Message = "Failed to create new task activity",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }

        public async Task<bool> DeleteTaskActivity(int id)
        {
            var existedTaskActivity = await _unitOfWork.TaskActivityRepository.Get(a => !a.Deleted && a.TaskActivityId == id).FirstOrDefaultAsync();
            if (existedTaskActivity is null)
            {
                return false;
            }
            existedTaskActivity.Deleted = true;
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(TaskActivity), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }

        public async Task<ServiceResponse<TaskActivity>> UpdateTaskActivity(UpdateTaskActivityVM updateTaskActivity, int id)
        {
            var existedTaskActivity = await _unitOfWork.TaskActivityRepository
                .Get(ta => !ta.Deleted && ta.TaskActivityId == id)
                .FirstOrDefaultAsync();

            if (existedTaskActivity is null)
            {
                return new ServiceResponse<TaskActivity>
                {
                    IsSuccess = false,
                    Message = "Failed to update activity",
                    Errors = new string[] { "Activity not found" }
                };
            }

            existedTaskActivity.ActivityTitle = updateTaskActivity.ActivityTitle ?? existedTaskActivity.ActivityTitle;
            existedTaskActivity.ActivityDescription = updateTaskActivity.ActivityDescription ?? existedTaskActivity.ActivityDescription;
            existedTaskActivity.Deadline = updateTaskActivity.Deadline ?? existedTaskActivity.Deadline;

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    string key1 = CacheKeyGenerator.GetKeyById(nameof(TaskLecturer), existedTaskActivity.TaskLecturerId.ToString());
                    string key2 = CacheKeyGenerator.GetKeyById(nameof(TaskActivity), existedTaskActivity.TaskActivityId.ToString());
                    var task1 = _cacheService.RemoveAsync(key1);
                    var task2 = _cacheService.RemoveAsync(key2);

                    return new ServiceResponse<TaskActivity>
                    {
                        Entity = existedTaskActivity,
                        IsSuccess = true,
                        Message = "Update activity successfully"
                    };
                }
                else
                {
                    return new ServiceResponse<TaskActivity>
                    {
                        IsSuccess = false,
                        Message = "Failed to update activity",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<TaskActivity>
                {
                    IsSuccess = false,
                    Message = "Failed to update activity",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<TaskActivity>
                {
                    IsSuccess = false,
                    Message = "Failed to update activity",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }
    }
}
