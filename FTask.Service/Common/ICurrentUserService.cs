namespace FTask.Repository.Common;

public interface ICurrentUserService
{
    public string UserId { get; }

    public string UserName { get; }

    IEnumerable<string> Roles { get; }
}
