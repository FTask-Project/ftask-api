using System.ComponentModel.DataAnnotations;
using TaskActivityVM = FTask.Service.ViewModel.RequestVM.CreateTask.TaskActivityVM;

namespace FTask.Service.ViewModel.RequestVM.CreateTaskLecturer;

public class CreateTaskLecturerVM
{
    public string? Note { get; set; }
    [Required]
    public int TaskId { get; set; }
    [Required]
    public Guid LecturerId { get; set; }
    public IEnumerable<TaskActivityVM> TaskActivities { get; set; } = new List<TaskActivityVM>();
}