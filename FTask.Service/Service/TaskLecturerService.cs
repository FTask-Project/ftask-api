using AutoMapper;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM.TaskLecturer;
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
        private readonly ICreateTaskValidation _createTaskValidation;

        public TaskLecturerService(
            IUnitOfWork unitOfWork, 
            ICheckQuantityTaken checkQuantityTaken, 
            ICacheService<TaskLecturer> cacheService, 
            IMapper mapper,
            ICreateTaskValidation createTaskValidation)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _mapper = mapper;
            _createTaskValidation = createTaskValidation;
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

            var includes = new Expression<Func<TaskLecturer, object>>[]
            {
                tl => tl.TaskActivities!
            };

            var taskLecturerList = _unitOfWork.TaskLecturerRepository
                .Get(tl => !tl.Deleted, includes)
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

        public async Task<ServiceResponse<TaskLecturer>> CreateNewTaskLecturer(CreateTaskLecturerVM newEntity)
        {
            var includes = new Expression<Func<FTask.Repository.Entity.Task, object>>[]
            {
                t => t.TaskLecturers
            };
            var exsitedTask = await _unitOfWork.TaskRepository
                .Get(t => !t.Deleted && t.TaskId == newEntity.TaskId, includes)
                .FirstOrDefaultAsync();
            if (exsitedTask is null)
            {
                return new ServiceResponse<TaskLecturer>
                {
                    IsSuccess = false,
                    Message = "Failed to assign task",
                    Errors = new string[1] { "Can not find task" }
                };
            }

            var task1 = _createTaskValidation.CanAssignTask(new Guid[] { newEntity.LecturerId }, _unitOfWork.DepartmentRepository);
            var task2 = _createTaskValidation.IsMaximumAssign(exsitedTask.TaskLecturers.Count() + 1);
            var canAssignTask = await task1;
            var checkMaximum = await task2;
            if(!canAssignTask.IsSuccess)
            {
                return new ServiceResponse<TaskLecturer>
                {
                    IsSuccess = false,
                    Message = canAssignTask.Message,
                    Errors = canAssignTask.Errors
                };
            }
            if (!checkMaximum.IsSuccess)
            {
                return new ServiceResponse<TaskLecturer>
                {
                    IsSuccess = false,
                    Message = checkMaximum.Message,
                    Errors = checkMaximum.Errors
                };
            }

            var existedLecturer = await _unitOfWork.LecturerRepository.FindAsync(newEntity.LecturerId);
            if (existedLecturer is null)
            {
                return new ServiceResponse<TaskLecturer>
                {
                    IsSuccess = false,
                    Message = "Failed to assign task",
                    Errors = new string[1] { "Lecturer not found" }
                };
            }

            var existedTaskLecturer = await _unitOfWork.TaskLecturerRepository
                .Get(tl => tl.TaskId == newEntity.TaskId && tl.LecturerId == newEntity.LecturerId)
                .FirstOrDefaultAsync();
            if (existedTaskLecturer is not null)
            {
                return new ServiceResponse<TaskLecturer>
                {
                    IsSuccess = false,
                    Message = "Failed to assign task",
                    Errors = new string[] { "Lecturers has been already assigned to the task before" }
                };
            }

            var newTaskLecturer = _mapper.Map<TaskLecturer>(newEntity);

            await _unitOfWork.TaskLecturerRepository.AddAsync(newTaskLecturer);

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    string key = CacheKeyGenerator.GetKeyById(nameof(FTask.Repository.Entity.Task), exsitedTask.TaskId.ToString());
                    var task = _cacheService.RemoveAsync(key);

                    return new ServiceResponse<TaskLecturer>
                    {
                        Entity = newTaskLecturer,
                        IsSuccess = true,
                        Message = "Assign task successfully"
                    };
                }
                else
                {
                    return new ServiceResponse<TaskLecturer>
                    {
                        IsSuccess = false,
                        Message = "Failed to assign task",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<TaskLecturer>
                {
                    IsSuccess = false,
                    Message = "Failed to assign task",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<TaskLecturer>
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
            if (existedTaskLecturer is null)
            {
                return false;
            }
            existedTaskLecturer.Deleted = true;
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(TaskLecturer), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }

        public async Task<ServiceResponse<TaskLecturer>> UpdateTaskLecturer(UpdateTaskLecturerVM updateTaskLecturer, int id)
        {
            var existedTaskLecturer = await _unitOfWork.TaskLecturerRepository
                .Get(tl => !tl.Deleted && tl.TaskLecturerId == id)
                .FirstOrDefaultAsync();

            if (existedTaskLecturer is null)
            {
                return new ServiceResponse<TaskLecturer>
                {
                    IsSuccess = false,
                    Message = "Failed to update",
                    Errors = new string[] { "The task has not been assigned to lecturer" }
                };
            }

            existedTaskLecturer.Note = updateTaskLecturer.Note ?? existedTaskLecturer.Note;

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    string key = CacheKeyGenerator.GetKeyById(nameof(TaskLecturer), existedTaskLecturer.TaskLecturerId.ToString());
                    var task = _cacheService.RemoveAsync(key);

                    return new ServiceResponse<TaskLecturer>
                    {
                        Entity = existedTaskLecturer,
                        IsSuccess = true,
                        Message = "Update successfully"
                    };
                }
                else
                {
                    return new ServiceResponse<TaskLecturer>
                    {
                        IsSuccess = false,
                        Message = "Failed to update",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<TaskLecturer>
                {
                    IsSuccess = false,
                    Message = "Failed to update",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<TaskLecturer>
                {
                    IsSuccess = false,
                    Message = "Failed to update",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }
    }
}
