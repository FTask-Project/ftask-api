using FTask.Repository.Common;
using System.Security.Claims;

namespace FTask.API.Service;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unauthenticated User";

    public string UserName =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ?? "Unauthenticated User";

    public IEnumerable<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.FindAll(c => c.Type == "Scope").Select(c => c.Value) ?? Enumerable.Empty<string>();
}
