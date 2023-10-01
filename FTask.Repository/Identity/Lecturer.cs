using FTask.Repository.Entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTask.Repository.Identity
{
    public class Lecturer : IdentityUser<Guid>
    {
        public string CreatedBy { get; set; } = "Undefined";
        public DateTime CreatedAt { get; set; }


        // Managed Department, this is for Manager
        public Department? ManagedDepartment { get; set; }

        // Belong to which department this is for lecturer
        public Department? Department { get; set; }
        public int? DepartmentId { get; set; }

        public IEnumerable<TaskLecturer>? TaskLecturer { get; set; }
        public IEnumerable<Subject>? Subjects { get; set; }
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
