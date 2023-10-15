using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Duende.IdentityServer.Extensions;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Validation;
using FTask.Service.ViewModel;
using FTask.Service.ViewModel.RequestVM.CreateTask;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Task = FTask.Repository.Entity.Task;

namespace FTask.Service.IService
{
    internal class TaskService : ITaskService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<Task> _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;

        public TaskService(
            IUnitOfWork unitOfWork,
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Task> cacheService,
            IMapper mapper,
            Cloudinary cloudinary)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _mapper = mapper;
            _cloudinary = cloudinary;
        }

        public async Task<Task?> GetTaskById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Task), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var task = await _unitOfWork.TaskRepository.FindAsync(id);
                if (task is not null)
                {
                    await _cacheService.SetAsync(key, task);
                }
                return task;
            }

            return cachedData;
        }

        public async Task<IEnumerable<Task>> GetTasks(int page, int quantity, string filter, int? semsesterId, int? departmentId, int? subjectId, int? status)
        {
            if (page == 0)
            {
                page = 1;
            }
            quantity = _checkQuantityTaken.check(quantity);

            var taskList = _unitOfWork.TaskRepository
                    .Get(t => t.TaskTitle.Contains(filter))
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity)
                    .AsNoTracking();

            if(semsesterId is not null)
            {
                taskList = taskList.Where(t => t.SemesterId == semsesterId);
            }

            if(departmentId is not null)
            {
                taskList = taskList.Where(t => t.DepartmentId == departmentId);
            }

            if (subjectId is not null)
            {
                taskList = taskList.Where(t => t.SubjectId == subjectId);
            }

            if(status is not null)
            {
                taskList = taskList.Where(t => t.TaskStatus == status);
            }

            return await taskList.ToArrayAsync();
        }

        public async Task<ServiceResponse> CreateNewTask(TaskVM newEntity)
        {
            int level = 1;

            if (newEntity.StartDate > newEntity.EndDate)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "End date must be greater than start date"
                };
            }

            if (newEntity.DepartmentId is not null)
            {
                var existedDepartment = await _unitOfWork.DepartmentRepository.FindAsync(newEntity.DepartmentId ?? 0);
                if (existedDepartment is null)
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Department not found",
                        Errors = new string[1] { "Department not found with the given id: " + newEntity.DepartmentId }
                    };
                }
                level = 2;
                if (newEntity.SubjectId is not null)
                {
                    var existedSubject = await _unitOfWork.SubjectRepository.FindAsync(newEntity.SubjectId ?? 0);
                    if (existedSubject is null)
                    {
                        return new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Create new task failed",
                            Errors = new string[1] { "Subject not found with the given id: " + newEntity.SubjectId }
                        };
                    }
                    level = 3;
                }
            }

            if (newEntity.TaskLecturers.Count() > 0)
            {
                foreach (var assignedlecturer in newEntity.TaskLecturers)
                {
                    var existedLecturer = await _unitOfWork.LecturerRepository.FindAsync(assignedlecturer.LecturerId);
                    if (existedLecturer is null)
                    {
                        return new ServiceResponse
                        {
                            IsSuccess = false,
                            Message = "Create new task failed",
                            Errors = new string[1] { "Lecturer not found wit the given id: " + assignedlecturer.LecturerId }
                        };
                    }
                }
            }

            var currentDateTime = DateTime.Now;
            var currentSemester = await _unitOfWork.SemesterRepository.Get(s => currentDateTime >= s.StartDate && currentDateTime <= s.EndDate).FirstOrDefaultAsync();
            if (currentSemester is null)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "New semester has not been started"
                };
            }

            var newTask = _mapper.Map<Task>(newEntity);
            newTask.TaskLevel = level;
            newTask.Semester = currentSemester;

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.3 * 2))
            };

            if (newEntity.Attachments.Count() > 0)
            {
                var errors = new ConcurrentQueue<string>();

                var uploadTasks = newEntity.Attachments
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
                        Message = "Create new task failed",
                        Errors = errors
                    };
                }
                if (urls.Count() > 0)
                {
                    var attachments = new List<Attachment>();
                    foreach (var url in urls)
                    {
                        Attachment attachment = new Attachment
                        {
                            Url = url!,
                        };
                        await _unitOfWork.AttachmentRepository.AddAsync(attachment);
                        attachments.Add(attachment);
                    }
                    newTask.Attachments = attachments;
                }
            }

            if (newTask.TaskLecturers.Count() > 0)
            {
                Parallel.ForEach(newTask.TaskLecturers, options, async taskLecturer =>
                {
                    await _unitOfWork.TaskLecturerRepository.AddAsync(taskLecturer);
                    foreach (var activity in taskLecturer.TaskActivities)
                    {
                        await _unitOfWork.TaskActivityRepository.AddAsync(activity);
                    }
                });
            }

            await _unitOfWork.TaskRepository.AddAsync(newTask);

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponse
                    {
                        Id = newTask.TaskId.ToString(),
                        IsSuccess = true,
                        Message = "Create new task successfully"
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        IsSuccess = false,
                        Message = "Create new task failed",
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
