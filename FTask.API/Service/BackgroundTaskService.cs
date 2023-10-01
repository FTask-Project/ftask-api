using FTask.Repository.Data;
using Hangfire;

namespace FTask.API.Service;

public interface IBackgroundTaskService
{
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
}
