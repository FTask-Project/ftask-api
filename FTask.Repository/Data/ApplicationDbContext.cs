using FTask.Repository.application;
using FTask.Repository.Entity;
using FTask.Repository.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task = FTask.Repository.Entity.Task;

namespace FTask.Repository.Data;

public interface IApplicationDbContext
{
    public DbSet<Lecturer> Lecturers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<TaskLecturer> TaskLecturers { get; set; }
    public DbSet<TaskReport> TaskReports { get; set; }
    public DbSet<TaskActivity> TaskActivities { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Evidence> Evidences { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
}

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

    public DbSet<Lecturer> Lecturers { get; set; } = null!;
    public DbSet<Task> Tasks { get; set; } = null!;
    public DbSet<TaskLecturer> TaskLecturers { get; set; } = null!;
    public DbSet<TaskReport> TaskReports { get; set; } = null!;
    public DbSet<TaskActivity> TaskActivities { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Semester> Semesters { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Evidence> Evidences { get; set; } = null!;
    public DbSet<Attachment> Attachments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Lecturer>(entity =>
        {
            entity.ToTable("Lecturer");

            entity.HasOne(u => u.Department).WithMany(d => d.Lecturers)
                .HasForeignKey(u => u.DepartmentId).OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(u => u.Subjects).WithMany(s => s.Lecturers)
                .UsingEntity("LecturerSubject",
                    l => l.HasOne(typeof(Subject)).WithMany().HasForeignKey("SubjectId").OnDelete(DeleteBehavior.NoAction).HasPrincipalKey(nameof(Subject.SubjectId)),
                    r => r.HasOne(typeof(Lecturer)).WithMany().HasForeignKey("LecturerId").OnDelete(DeleteBehavior.NoAction).HasPrincipalKey(nameof(Lecturer.Id)),
                    j =>
                    {
                        j.HasKey("SubjectId", "LecturerId");
                    });

            entity.HasOne(u => u.DepartmentHead).WithOne(d => d.DepartmentHead)
                .HasForeignKey<Department>(d => d.DepartmentHeadId);

            entity.HasIndex(l => l.Email).IsUnique();

            entity.HasIndex(l => l.PhoneNumber).IsUnique();
        });

        builder.Entity<User>(entity =>
        {
            entity.HasMany(u => u.Roles).WithMany(r => r.Users)
                .UsingEntity<IdentityUserRole<Guid>>();

            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasIndex(u => u.PhoneNumber).IsUnique();
        });

        builder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");

            entity.HasIndex(d => d.DepartmentCode).IsUnique();
        });

        builder.Entity<Semester>(entity =>
        {
            entity.ToTable("Semester");

            entity.HasIndex(s => s.SemesterCode).IsUnique();
        });

        builder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subject");

            entity.HasIndex(s => s.SubjectCode).IsUnique();

            entity.HasMany(s => s.Tasks).WithOne(t => t.Subject).HasForeignKey(t => t.SubjectId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Task>(entity =>
        {
            entity.ToTable("Task");

            entity.HasMany(t => t.TaskLecturers).WithOne(tu => tu.Task)
                .HasForeignKey(tu => tu.TaskId).OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<TaskActivity>(entity =>
        {
            entity.ToTable("TaskActivity");
        });

        builder.Entity<TaskReport>(entity =>
        {
            entity.ToTable("TaskReport");

            entity.HasOne(r => r.TaskActivity).WithOne(a => a.TaskReport)
                .HasForeignKey<TaskReport>(r => r.TaskActivityId);
        });

        builder.Entity<TaskLecturer>(entity =>
        {
            entity.ToTable("TaskLecturer");
        });

        builder.Entity<Evidence>(entity =>
        {
            entity.ToTable("Evidence");
        });

        builder.Entity<Attachment>(entity =>
        {
            entity.ToTable("Attachment");
        });

        builder.Ignore<IdentityUserClaim<Guid>>();
        builder.Ignore<IdentityUserLogin<Guid>>();
        builder.Ignore<IdentityUserToken<Guid>>();
        builder.Ignore<IdentityRoleClaim<Guid>>();
    }
}
