﻿using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel.RequestVM.CreateSemester;

public class SemesterVM
{
    [Required]
    public string SemesterCode { get; set; } = default!;
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
}
