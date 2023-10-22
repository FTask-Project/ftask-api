using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel.RequestVM.Role;

public class RoleVM
{
    [Required]
    public string? RoleName { get; set; }
}

public class UpdateRoleVM
{
    public string? RoleName { get; set; }
}
