using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateTaskReport;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace FTask.Service.IService
{
    internal class TaskReportService : ITaskReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<TaskReport> _cacheService;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;

        public TaskReportService(IUnitOfWork unitOfWork, ICheckQuantityTaken checkQuantityTaken, ICacheService<TaskReport> cacheService, IMapper mapper, Cloudinary cloudinary)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _mapper = mapper;
            _cloudinary = cloudinary;
        }

        public async Task<TaskReport?> GetTaskReportById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(TaskReport), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var taskReport = await _unitOfWork.TaskReportRepository.FindAsync(id);
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
                .FindAll()
                .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                .Take(quantity);

            if (taskActivityId is not null)
            {
                taskReportList = taskReportList.Where(r => r.TaskActivityId == taskActivityId);
            }

            return await taskReportList.ToArrayAsync();
        }

        public async Task<ServiceResponse> CreateNewTaskReport(TaskReportVM newEntity)
        {
            var existedTaskActivity = await _unitOfWork.TaskActivityRepository.FindAsync(newEntity.TaskActivityId);
            if (existedTaskActivity is null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Failed to create new task report",
                    Errors = new string[1] { "Can not find provided task activity" }
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
                        return uploadResult.SecureUrl.ToString();
                    });

                var urls = await System.Threading.Tasks.Task.WhenAll(uploadTasks);

                if (errors.Count() > 0)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Failed to create new task report",
                        Errors = errors
                    };
                }
                if (urls.Count() > 0)
                {
                    var evidences = new List<Evidence>();
                    foreach (var url in urls)
                    {
                        Evidence evidence = new Evidence
                        {
                            Url = url!,
                        };
                        await _unitOfWork.EvidenceRepository.AddAsync(evidence);
                        evidences.Add(evidence);
                    }
                    newTaskReport.Evidences = evidences;
                }
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
        }
    }
}
