using FTask.Repository.Data;
using FTask.Service.Enum;
using Hangfire;

namespace FTask.Service.Service;

public interface IBackgroundTaskService
{
    Task SetInProgressTask(int taskId);
    Task SetEndTask(int taskId);
    Task SetOverdueTaskActivity(int taskActivityId);
}

internal class BackgroundTaskService : IBackgroundTaskService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IUnitOfWork _unitOfWork;

    public BackgroundTaskService(IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, IUnitOfWork unitOfWork)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _unitOfWork = unitOfWork;
    }

    public async Task SetInProgressTask(int taskId)
    {
        var task = await _unitOfWork.TaskRepository.FindAsync(taskId);
        if(task != null)
        {
            _backgroundJobClient.Schedule(() => SetTaskStatus(task.TaskId, (int)FTask.Service.Enum.TaskStatus.InProgress), (TimeSpan)(task.StartDate - DateTime.Now));
        }
    }

    public async Task SetEndTask(int taskId)
    {
        var task = await _unitOfWork.TaskRepository.FindAsync(taskId);
        if (task != null)
        {
            _backgroundJobClient.Schedule(() => SetTaskStatus(task.TaskId, (int)FTask.Service.Enum.TaskStatus.End), (TimeSpan)(task.EndDate - DateTime.Now));
        }
    }

    public async Task SetOverdueTaskActivity(int taskActivityId)
    {
        var taskActivity = await _unitOfWork.TaskActivityRepository.FindAsync(taskActivityId);
        if(taskActivity is not null)
        {
            _backgroundJobClient.Schedule(() => SetTaskActivityStatus(taskActivity.TaskActivityId, (int)TaskActivityStatus.Overdue), (TimeSpan)(taskActivity.Deadline - DateTime.Now));
        }
    }

    public async Task SetTaskStatus(int taskId, int taskStatus)
    {
        var task = await _unitOfWork.TaskRepository.FindAsync(taskId);
        if(task is not null)
        {
            task.TaskStatus = taskStatus;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task SetTaskActivityStatus(int taskActivityId, int taskActivityStatus)
    {
        var taskActivity = await _unitOfWork.TaskActivityRepository.FindAsync(taskActivityId);
        if(taskActivity is not null)
        {
            if(taskActivityStatus == (int)TaskActivityStatus.Overdue && taskActivity.TaskActivityStatus == (int)TaskActivityStatus.InProgress)
            {
                taskActivity.TaskActivityStatus = taskActivityStatus;
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
