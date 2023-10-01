using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel;

public class AuthenticateResponseVM
{
    public string? Token { get; set; }
    public UserInformationResponseVM? UserInformation { get; set; }
}

public class UserInformationResponseVM
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
}
