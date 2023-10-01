using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel;

public class LoginUserVM
{
    [Required]
    public string UserName { get; set; } = "";

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "";

    public bool IsLecturer { get; set; } = false;

    public bool IsMember { get; set; } = false;
}