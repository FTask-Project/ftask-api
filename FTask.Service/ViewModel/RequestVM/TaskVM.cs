using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FTask.Service.ViewModel.RequestVM.Task;

public class TaskVM
{
    [Required]
    public string TaskTitle { get; set; } = "No Title";
    public string? TaskContent { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public int? SubjectId { get; set; }
    public int? DepartmentId { get; set; }
    public IEnumerable<TaskLecturerVM> TaskLecturers { get; set; } = new List<TaskLecturerVM>();
    public IEnumerable<IFormFile> Attachments { get; set; } = new List<IFormFile>();
    public IEnumerable<FilePathVM> FilePaths { get; set; } = new List<FilePathVM>();
}

public class FilePathVM
{
    public string FileName { get; set; } = "Undefined";
    public string Url { get; set; } = "Undefined";
}

public class TaskLecturerVM
{
    [Required]
    public Guid LecturerId { get; set; }
    public string? Note { get; set; }
    public IEnumerable<TaskActivityVM> TaskActivities { get; set; } = new List<TaskActivityVM>();
}

public class TaskActivityVM
{
    public string ActivityTitle { get; set; } = "No Title";
    public string ActivityDescription { get; set; } = "No description";
    public DateTime Deadline { get; set; }
}

public class UpdateTaskVM
{
    public string? TaskTitle { get; set; }
    public string? TaskContent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public int? SubjectId { get; set; }
    public int? DepartmentId { get; set; }
    public IEnumerable<IFormFile> AddAttachments { get; set; } = new List<IFormFile>();
    public IEnumerable<int> DeleteAttachment { get; set; } = new List<int>();
    public IEnumerable<FilePathVM> AddedFilePaths { get; set; } = new List<FilePathVM>();
}