using FTask.Repository.IRepository;
using FTask.Service.IService;
using FTask.Service.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FTask.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddService(this IServiceCollection services, IConfiguration configuration)
    {

        #region Services
        // Register services
        services.AddScoped<IAttachmentService,AttachmentService>();
        services.AddScoped<IDepartmentService,DepartmentService>();
        services.AddScoped<IEvidenceService,EvidenceService>();
        services.AddScoped<ILecturerService,LecturerService>();
        services.AddScoped<IRoleService,RoleService>();
        services.AddScoped<ISemesterService,SemesterService>();
        services.AddScoped<IUserService,UserService>();
        services.AddScoped<ISubjectService,SubjectService>();
        services.AddScoped<ITaskActivityService,TaskActivityService>();
        services.AddScoped<ITaskLecturerService,TaskLecturerService>();
        services.AddScoped<ITaskReportService,TaskReportService>();
        services.AddScoped<ITaskService,TaskService>();
        #endregion

        #region Validation
        services.AddSingleton<ICheckQuantityTaken, CheckQuantityTaken>();
        services.AddSingleton<ICheckSemesterPeriod, CheckSemesterPeriod>();
        #endregion

        return services;
    }
}
