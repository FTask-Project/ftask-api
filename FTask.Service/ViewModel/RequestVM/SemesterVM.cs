using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel.RequestVM.Semester;

public class SemesterVM
{
    [Required]
    public string SemesterCode { get; set; } = default!;
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
}

public class UpdateSemesterVM
{
    public string? SemesterCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
