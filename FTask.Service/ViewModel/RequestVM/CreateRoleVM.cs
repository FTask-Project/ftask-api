using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel.RequestVM.CreateRole;

public class RoleVM
{
    [Required]
    public string? RoleName { get; set; }
}
