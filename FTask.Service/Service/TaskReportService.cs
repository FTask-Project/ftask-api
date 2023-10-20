using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Enum;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM.CreateTaskReport;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace FTask.Service.IService
{
    internal class TaskReportService : ITaskReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<TaskReport> _cacheService;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;
        private readonly ICurrentUserService _currentUserService;

        public TaskReportService(IUnitOfWork unitOfWork, ICheckQuantityTaken checkQuantityTaken, ICacheService<TaskReport> cacheService, IMapper mapper, Cloudinary cloudinary, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _mapper = mapper;
            _cloudinary = cloudinary;
            _currentUserService = currentUserService;
        }

        public async Task<TaskReport?> GetTaskReportById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(TaskReport), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var include = new Expression<Func<TaskReport, object>>[]
                {
                    tr => tr.TaskActivity!,
                    tr => tr.Evidences
                };
                var taskReport = await _unitOfWork.TaskReportRepository
                    .Get(tr => !tr.Deleted && tr.TaskReportId == id, include)
                    .FirstOrDefaultAsync();
                if (taskReport is not null)
                {
                    await _cacheService.SetAsync(key, taskReport);
                }
                return taskReport;
            }

            return cachedData;
        }

        public async Task<IEnumerable<TaskReport>> GetTaskReports(int page, int quantity, int? taskActivityId)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            var taskReportList = _unitOfWork.TaskReportRepository
                .Get(tr => !tr.Deleted)
                .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                .Take(quantity)
                .AsNoTracking();

            if (taskActivityId is not null)
            {
                taskReportList = taskReportList.Where(r => r.TaskActivityId == taskActivityId);
            }

            return await taskReportList.ToArrayAsync();
        }

        public async Task<ServiceResponse> CreateNewTaskReport(TaskReportVM newEntity)
        {
            var existedTaskActivity = await _unitOfWork.TaskActivityRepository
                .Get(ta => ta.TaskActivityId == newEntity.TaskActivityId, new Expression<Func<TaskActivity, object>>[]
                {
                    ta => ta.TaskReport!
                }).FirstOrDefaultAsync();

            if (existedTaskActivity is null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new task report",
                    Errors = new string[1] { "Can not find task activity" }
                };
            }

            if(existedTaskActivity.TaskReport is not null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new task report",
                    Errors = new string[1] { "You already made task report for this activity" }
                };
            }

            var newTaskReport = _mapper.Map<TaskReport>(newEntity);

            if (newEntity.Evidences.Count() > 0)
            {
                var errors = new ConcurrentQueue<string>();

                var uploadTasks = newEntity.Evidences
                    .Where(file => file is not null && file.Length > 0)
                    .Select(async file =>
                    {
                        var uploadFile = new RawUploadParams
                        {
                            File = new FileDescription(file.FileName, file.OpenReadStream())
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadFile);
                        if (uploadResult.Error is not null)
                        {
                            errors.Enqueue(uploadResult.Error.Message);
                            return null;
                        }
                        return new
                        {
                            Url = uploadResult.SecureUrl.ToString(),
                            FileName = file.FileName
                        };
                    });

                var uploadResult = await System.Threading.Tasks.Task.WhenAll(uploadTasks);

                if (errors.Count() > 0)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new task report",
                        Errors = errors
                    };
                }
                if (uploadResult.Count() > 0)
                {
                    var evidences = new List<Evidence>();
                    foreach (var item in uploadResult)
                    {
                        Evidence evidence = new Evidence
                        {
                            Url = item!.Url,
                            FileName = item!.FileName,
                            CreatedAt = DateTime.Now,
                            CreatedBy = _currentUserService.UserId
                        };
                        await _unitOfWork.EvidenceRepository.AddAsync(evidence);
                        evidences.Add(evidence);
                    }
                    newTaskReport.Evidences = evidences;
                }
            }

            if(existedTaskActivity.TaskActivityStatus != (int)TaskActivityStatus.Overdue && existedTaskActivity.TaskActivityStatus != (int)TaskActivityStatus.Done)
            {
                existedTaskActivity.TaskActivityStatus = (int)TaskActivityStatus.Done;
                _unitOfWork.TaskActivityRepository.Update(existedTaskActivity);

                // Need to remove cache data after update
                string key = CacheKeyGenerator.GetKeyById(nameof(TaskActivity), existedTaskActivity.TaskActivityId.ToString());
                var task = _cacheService.RemoveAsync(key);
            }

            await _unitOfWork.TaskReportRepository.AddAsync(newTaskReport);

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponse
                    {
                        Id = newTaskReport.TaskReportId.ToString(),
                        IsSuccess = true,
                        Message = "Create new task report successfully"
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new task report",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new task report",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new task report",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }

        public async Task<bool> DeleteTaskReport(int id)
        {
            var existedTaskReport = await _unitOfWork.TaskReportRepository.Get(tr => !tr.Deleted && tr.TaskReportId == id).FirstOrDefaultAsync();
            if(existedTaskReport is null)
            {
                return false;
            }
            _unitOfWork.TaskReportRepository.Remove(existedTaskReport);
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(TaskReport), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }
    }
}
