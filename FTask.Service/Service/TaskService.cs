using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FTask.Repository.Common;
using FTask.Repository.Data;
using FTask.Repository.Entity;
using FTask.Service.Caching;
using FTask.Service.Enum;
using FTask.Service.Validation;
using FTask.Service.ViewModel.RequestVM.Task;
using FTask.Service.ViewModel.ResposneVM;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Task = FTask.Repository.Entity.Task;
using TaskStatus = FTask.Service.Enum.TaskStatus;

namespace FTask.Service.IService
{
    internal class TaskService : ITaskService
    {
        private readonly ICheckQuantityTaken _checkQuantityTaken;
        private readonly ICacheService<Task> _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;
        private readonly ICreateTaskValidation _createTaskValidation;
        private readonly ICurrentUserService _currentUserService;

        public TaskService(
            IUnitOfWork unitOfWork,
            ICheckQuantityTaken checkQuantityTaken,
            ICacheService<Task> cacheService,
            IMapper mapper,
            Cloudinary cloudinary,
            ICreateTaskValidation createTaskValidation,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _checkQuantityTaken = checkQuantityTaken;
            _cacheService = cacheService;
            _mapper = mapper;
            _cloudinary = cloudinary;
            _createTaskValidation = createTaskValidation;
            _currentUserService = currentUserService;
        }

        public async Task<Task?> GetTaskById(int id)
        {
            string key = CacheKeyGenerator.GetKeyById(nameof(Task), id.ToString());
            var cachedData = await _cacheService.GetAsync(key);

            if (cachedData is null)
            {
                var include = new Expression<Func<Task, object>>[]
                {
                    t => t.Semester!,
                    t => t.Department!,
                    t => t.Subject!,
                    t => t.Attachments,
                };
                var task = await _unitOfWork.TaskRepository
                    .Get(t => !t.Deleted && t.TaskId == id, include)
                    .Include(nameof(Task.TaskLecturers) + "." + nameof(TaskLecturer.Lecturer))
                    .Include(nameof(Task.TaskLecturers) + "." + nameof(TaskLecturer.TaskActivities))
                    .FirstOrDefaultAsync();
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
                    .Get(t => !t.Deleted && t.TaskTitle.Contains(filter))
                    .Skip((page - 1) * _checkQuantityTaken.PageQuantity)
                    .Take(quantity)
                    .AsNoTracking();

            if (semsesterId is not null)
            {
                taskList = taskList.Where(t => t.SemesterId == semsesterId);
            }

            if (departmentId is not null)
            {
                taskList = taskList.Where(t => t.DepartmentId == departmentId);
            }

            if (subjectId is not null)
            {
                taskList = taskList.Where(t => t.SubjectId == subjectId);
            }

            if (status is not null)
            {
                taskList = taskList.Where(t => t.TaskStatus == status);
            }

            return await taskList.ToArrayAsync();
        }

        public async Task<ServiceResponse<Task>> CreateNewTask(TaskVM newEntity)
        {
            var canAssignTask = _createTaskValidation.CanAssignTask(newEntity.TaskLecturers.Select(t => t.LecturerId), _unitOfWork.DepartmentRepository);
            var checkMaximum = _createTaskValidation.IsMaximumAssign(newEntity.TaskLecturers.Count());
            if (!(await canAssignTask).IsSuccess)
            {
                return await canAssignTask;
            }
            if(!(await checkMaximum).IsSuccess)
            {
                return await checkMaximum;
            }

            var currentDateTime = DateTime.Now;
            var currentSemester = await _unitOfWork.SemesterRepository.Get(s => currentDateTime >= s.StartDate && currentDateTime <= s.EndDate).FirstOrDefaultAsync();
            if (currentSemester is null)
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = false,
                    Message = "Failed to create new task",
                    Errors = new string[1] { "New Semester has not yet started" }
                };
            }

            if (newEntity.StartDate > newEntity.EndDate)
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = false,
                    Message = "Failed to create new task",
                    Errors = new string[1] { "End date must be greater than start date" }
                };
            }

            if (newEntity.TaskLecturers.Count() > 0)
            {
                foreach (var assignedlecturer in newEntity.TaskLecturers)
                {
                    var existedLecturer = await _unitOfWork.LecturerRepository.FindAsync(assignedlecturer.LecturerId);
                    if (existedLecturer is null)
                    {
                        return new ServiceResponse<Task>
                        {
                            IsSuccess = false,
                            Message = "Failed to create new task",
                            Errors = new string[1] { "Lecturer not found" }
                        };
                    }
                }
            }

            var newTask = _mapper.Map<Task>(newEntity);

            int level = (int)TaskLevel.Semester;
            if (newEntity.DepartmentId is not null)
            {
                var existedDepartment = await _unitOfWork.DepartmentRepository
                    .Get(d => !d.Deleted && d.DepartmentId == newEntity.DepartmentId)
                    .FirstOrDefaultAsync();
                if (existedDepartment is null)
                {
                    return new ServiceResponse<Task>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new task",
                        Errors = new string[1] { "Department not found " }
                    };
                }
                newTask.Department = existedDepartment;
                level = (int)TaskLevel.Department;
                if (newEntity.SubjectId is not null)
                {
                    var existedSubject = await _unitOfWork.SubjectRepository
                        .Get(s => !s.Deleted && s.SubjectId == newEntity.SubjectId)
                        .FirstOrDefaultAsync();
                    if (existedSubject is null)
                    {
                        return new ServiceResponse<Task>
                        {
                            IsSuccess = false,
                            Message = "Failed to create new task",
                            Errors = new string[1] { "Subject not found" }
                        };
                    }
                    if (existedSubject.DepartmentId != existedDepartment.DepartmentId)
                    {
                        return new ServiceResponse<Task>
                        {
                            IsSuccess = false,
                            Message = "Failed to create new task",
                            Errors = new string[1] { $"Subject '{existedSubject.SubjectName}' does not belong to '{existedDepartment.DepartmentName}'" }
                        };
                    }
                    newTask.Subject = existedSubject;
                    level = (int)TaskLevel.Subject;
                }
            }

            newTask.TaskLevel = level;
            newTask.Semester = currentSemester;

            if (currentDateTime >= newTask.StartDate)
            {
                newTask.TaskStatus = (int)TaskStatus.InProgress;
            }
            else
            {
                newTask.TaskStatus = (int)TaskStatus.ToDo;
            }

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
                        return new
                        {
                            Url = uploadResult.SecureUrl.ToString(),
                            FileName = file.FileName
                        };
                        //return uploadResult.SecureUrl.ToString();
                    });

                var result = await System.Threading.Tasks.Task.WhenAll(uploadTasks);

                if (errors.Count() > 0)
                {
                    return new ServiceResponse<Task>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new task",
                        Errors = errors
                    };
                }
                if (result.Count() > 0)
                {
                    var attachments = new List<Attachment>();
                    foreach (var item in result)
                    {
                        Attachment attachment = new Attachment
                        {
                            Url = item!.Url,
                            FileName = item!.FileName,
                            CreatedBy = _currentUserService.UserId,
                            CreatedAt = DateTime.Now
                        };
                        await _unitOfWork.AttachmentRepository.AddAsync(attachment);
                        attachments.Add(attachment);
                    }
                    newTask.Attachments = attachments;
                }
            }

            await _unitOfWork.TaskRepository.AddAsync(newTask);

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponse<Task>
                    {
                        Entity = newTask,
                        IsSuccess = true,
                        Message = "Create new task successfully"
                    };
                }
                else
                {
                    return new ServiceResponse<Task>
                    {
                        IsSuccess = false,
                        Message = "Failed to create new task",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = false,
                    Message = "Failed to create new task",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = false,
                    Message = "Failed to create new task",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }

        public async Task<bool> DeleteTask(int id)
        {
            var existedTask = await _unitOfWork.TaskRepository.Get(t => !t.Deleted && t.TaskId == id).FirstOrDefaultAsync();
            if (existedTask is null)
            {
                return false;
            }
            existedTask.Deleted = true;
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                string key = CacheKeyGenerator.GetKeyById(nameof(Task), id.ToString());
                await _cacheService.RemoveAsync(key);
            }

            return result;
        }

        public async Task<ServiceResponse<Task>> UpdateTask(UpdateTaskVM updateTask, int id)
        {
            var includes = new Expression<Func<Task, object>>[]
            {
                t => t.Semester!,
                t => t.Department!,
                t => t.Subject!,
                t => t.Attachments
            };
            var existedTask = _unitOfWork.TaskRepository
                .Get(t => !t.Deleted && t.TaskId == id, includes)
                .FirstOrDefault();
            if (existedTask is null)
            {
                return new ServiceResponse<Repository.Entity.Task>
                {
                    IsSuccess = false,
                    Message = "Failed to update task",
                    Errors = new string[] { "Task not found" }
                };
            }

            var attachments = existedTask.Attachments.ToList();

            var startDate = updateTask.StartDate ?? existedTask.StartDate;
            var endDate = updateTask.EndDate ?? existedTask.EndDate;

            if (startDate > endDate)
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = false,
                    Message = "Failed to update task",
                    Errors = new string[1] { "End date must be greater than start date" }
                };
            }

            if (updateTask.DepartmentId is not null)
            {
                if (updateTask.DepartmentId == 0)
                {
                    existedTask.DepartmentId = null;
                }
                else
                {
                    var existedDepartment = await _unitOfWork.DepartmentRepository
                    .Get(d => !d.Deleted && d.DepartmentId == updateTask.DepartmentId)
                    .FirstOrDefaultAsync();

                    if (existedDepartment is null)
                    {
                        return new ServiceResponse<Repository.Entity.Task>
                        {
                            IsSuccess = false,
                            Message = "Failed to update task",
                            Errors = new string[] { "Department not found" }
                        };
                    }

                    existedTask.DepartmentId = updateTask.DepartmentId;
                    existedTask.Department = existedDepartment;
                }
            }

            if (updateTask.SubjectId is not null)
            {
                if (updateTask.SubjectId == 0)
                {
                    existedTask.SubjectId = null;
                }
                else
                {
                    if (existedTask.DepartmentId is not null)
                    {
                        var existedSubject = await _unitOfWork.SubjectRepository
                            .Get(s => !s.Deleted && s.SubjectId == updateTask.SubjectId)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();

                        if (existedSubject is null)
                        {
                            return new ServiceResponse<Repository.Entity.Task>
                            {
                                IsSuccess = false,
                                Message = "Failed to update task",
                                Errors = new string[] { "Subject not found" }
                            };
                        }

                        if (existedSubject.DepartmentId != existedTask.DepartmentId)
                        {
                            return new ServiceResponse<Repository.Entity.Task>
                            {
                                IsSuccess = false,
                                Message = "Failed to update task",
                                Errors = new string[] { $"Subejct '{existedSubject.SubjectName}' does not belong to '{existedTask.Department!.DepartmentName}'" }
                            };
                        }

                        existedTask.SubjectId = updateTask.SubjectId;
                        existedTask.Subject = existedSubject;
                    }
                }
            }

            if (existedTask.DepartmentId is not null)
            {
                if (existedTask.SubjectId is not null)
                {
                    existedTask.TaskLevel = (int)TaskLevel.Subject;
                }
                else
                {
                    existedTask.TaskLevel = (int)TaskLevel.Department;
                }
            }
            else
            {
                existedTask.TaskLevel = (int)TaskLevel.Semester;
            }

            if (updateTask.DeleteAttachment.Count() > 0)
            {
                foreach (var attachment in attachments)
                {
                    if (updateTask.DeleteAttachment.Contains(attachment.AttachmentId))
                    {
                        attachment.Deleted = true;
                    }
                }
            }

            if (updateTask.AddAttachments.Count() > 0)
            {
                var errors = new ConcurrentQueue<string>();

                var uploadFiles = updateTask.AddAttachments
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

                var result = await System.Threading.Tasks.Task.WhenAll(uploadFiles);

                if (errors.Count() > 0)
                {
                    return new ServiceResponse<Task>
                    {
                        IsSuccess = false,
                        Message = "Failed to update task",
                        Errors = errors
                    };
                }

                if (result.Count() > 0)
                {
                    foreach (var item in result)
                    {
                        Attachment attachment = new Attachment
                        {
                            Url = item!.Url,
                            FileName = item!.FileName,
                            CreatedBy = _currentUserService.UserId,
                            CreatedAt = DateTime.Now
                        };
                        attachments.Add(attachment);
                    }
                }
            }

            existedTask.Attachments = attachments;

            existedTask.TaskTitle = updateTask.TaskTitle ?? existedTask.TaskTitle;
            existedTask.TaskContent = updateTask.TaskContent ?? existedTask.TaskContent;
            existedTask.Location = updateTask.Location ?? existedTask.Location;

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    string key = CacheKeyGenerator.GetKeyById(nameof(Task), existedTask.TaskId.ToString());
                    var task = _cacheService.RemoveAsync(key);

                    return new ServiceResponse<Task>
                    {
                        Entity = existedTask,
                        IsSuccess = true,
                        Message = "Update task successfully"
                    };
                }
                else
                {
                    return new ServiceResponse<Task>
                    {
                        IsSuccess = false,
                        Message = "Failed to update task",
                        Errors = new string[1] { "Can not save changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = false,
                    Message = "Failed to update task",
                    Errors = new List<string>() { ex.Message }
                };
            }
            catch (OperationCanceledException)
            {
                return new ServiceResponse<Task>
                {
                    IsSuccess = false,
                    Message = "Failed to update task",
                    Errors = new string[1] { "The operation has been cancelled" }
                };
            }
        }
    }
}
