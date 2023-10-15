using AutoMapper;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateTaskLecturer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var taskLecturer = await _unitOfWork.TaskLecturerRepository.FindAsync(id);
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
                .FindAll()
                .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                .Take(quantity)
                .AsNoTracking();

            if(taskId is not null)
            {
                taskLecturerList = taskLecturerList.Where(tl => tl.TaskId == taskId);
            }

            if(lecturerId is not null)
            {
                taskLecturerList = taskLecturerList.Where(tl => tl.LecturerId == lecturerId);
            }

            return await taskLecturerList.ToArrayAsync();
        }

        public async Task<ServiceResponse> CreateNewTaskLecturer(CreateTaskLecturerVM newEntity)
        {
            var exsitedTask = await _unitOfWork.TaskRepository.FindAsync(newEntity.TaskId);
            if(exsitedTask is null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Can not find task"
                };
            }

            var existedLecturer = await _unitOfWork.LecturerRepository.FindAsync(newEntity.LecturerId);
            if(existedLecturer is null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Can not find lecturer"
                };
            }

            var newTaskLecturer = _mapper.Map<TaskLecturer>(newEntity);

            var taskActivities = new List<TaskActivity>();
            if(newTaskLecturer.TaskActivities.Count() > 0)
            {
                foreach(var activity in newTaskLecturer.TaskActivities)
                {
                    taskActivities.Add(activity);
                }
            }

            await _unitOfWork.TaskActivityRepository.AddRangeAsync(taskActivities);
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
                        Message = "Assign task failed",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Create new task failed",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }
    }
}
