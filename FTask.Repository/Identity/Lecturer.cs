using FTask.Repository.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FTask.Repository.Identity
{
    public class Lecturer : IdentityUser<Guid>
    {
        public string CreatedBy { get; set; } = "Undefined";
        public DateTime CreatedAt { get; set; }

        public string? DisplayName { get; set; } = "Undefined";

        // Department this Lecturer is head of (if any)
        public Department? DepartmentHead { get; set; }

        // Belong to which department this is for lecturer
        public Department? Department { get; set; }
        public int? DepartmentId { get; set; }

        public IEnumerable<TaskLecturer> TaskLecturers { get; set; } = new List<TaskLecturer>();
        public IEnumerable<Subject> Subjects { get; set; } = new List<Subject>();

        public string? FilePath { get; set; }

        public bool Deleted { get; set; } = false;
    }

    public class LoginLecturerManagement
    {
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public Lecturer? LoginUser { get; set; }
        public string? ConfirmEmailUrl { get; set; }
    }
}
