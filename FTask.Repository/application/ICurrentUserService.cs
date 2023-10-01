namespace FTask.Repository.application;

public interface ICurrentUserService
{
    public Guid UserId { get; }

    public string UserName { get; }
}
