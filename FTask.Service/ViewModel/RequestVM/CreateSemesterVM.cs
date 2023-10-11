using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Service.ViewModel.RequestVM.CreateSemester;

public class SemesterVM
{
    [Required]
    public string? SemesterCode { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
}
