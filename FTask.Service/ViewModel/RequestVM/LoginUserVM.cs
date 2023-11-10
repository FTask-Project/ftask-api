using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel.RequestVM
{
    public class LoginUserVM
    {
        [Required]
        public string UserName { get; set; } = "";

        [Required]
        [MinLength(5)]
        public string Password { get; set; } = "";

        public string? DeviceToken { get; set; }
    }
}
