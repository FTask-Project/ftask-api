using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel.RequestVM.CreateTaskActivity;

public class CreateTaskActivityVM
{
    [Required]
    public string ActivityTitle { get; set; } = null!;
    public string? ActivityDescription { get; set; }
    public DateTime Deadline { get; set; }
    [Required]
    public int TaskLecturerId { get; set; }
}
