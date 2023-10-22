using Microsoft.AspNetCore.Identity;

namespace FTask.Repository.Identity;

public class User : IdentityUser<Guid>
{
    public string? DisplayName { get; set; } = "Undefined";
    public string CreatedBy { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }

    public IEnumerable<Role> Roles { get; set; } = new List<Role>();

    public string? FilePath { get; set; }

    public bool Deleted { get; set; } = false;
}

public class LoginUserManagement
{
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public User? LoginUser { get; set; }
    public string? ConfirmEmailUrl { get; set; }
    public IEnumerable<string>? RoleNames { get; set; }
}
