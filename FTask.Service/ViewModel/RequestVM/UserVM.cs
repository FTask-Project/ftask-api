using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FTask.Service.ViewModel.RequestVM.User;

public class UserVM
{
    [Required]
    public string UserName { get; set; } = "Undefined";
    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "Undefined";
    public string? PhoneNumber { get; set; }
    public bool? LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    [EmailAddress]
    [AllowNull]
    public string? Email { get; set; }
    public IEnumerable<Guid> RoleIds { get; set; } = new List<Guid>();
    public IFormFile? Avatar { get; set; }
    public string? FilePath { get; set; }
}

public class UpdateUserVM
{
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public IEnumerable<Guid>? RoleIds { get; set; }
    public IFormFile? Avatar { get; set; }
    public string? FilePath { get; set; }
}
