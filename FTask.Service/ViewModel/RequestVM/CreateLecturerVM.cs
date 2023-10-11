using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.ViewModel.RequestVM.CreateLecturer;

public class LecturerVM
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
    public int? DepartmentId { get; set; }
    public IEnumerable<int> SubjectIds { get; set; } = new List<int>();
    public IFormFile? Avatar { get; set; }
}
