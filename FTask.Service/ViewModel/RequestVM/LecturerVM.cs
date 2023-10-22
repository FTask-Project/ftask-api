using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FTask.Service.ViewModel.RequestVM.Lecturer;

public class LecturerVM
{
    [Required]
    public string UserName { get; set; } = "Undefined";
    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "Undefined";
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    [EmailAddress]
    [AllowNull]
    public string? Email { get; set; }
    public int? DepartmentId { get; set; }
    public IEnumerable<int> SubjectIds { get; set; } = new List<int>();
    public IFormFile? Avatar { get; set; }
}

public class UpdateLecturerVM
{
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public int? DepartmentId { get; set; }
    public IEnumerable<int>? SubjectIds { get; set; }
    public IFormFile? Avatar { get; set; }
}
