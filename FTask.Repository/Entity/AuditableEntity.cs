using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations.Schema;

namespace FTask.Repository.Entity;

public abstract class AuditableEntity
{
    public int Id { get; set; }
    public int Type { get; set; }
    public string? CreatedBy { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? TableName { get; set; }
    public string? PrimaryKey { get; set; }
    public Dictionary<string, object>? Changes { get; set; }
    public bool? IsRestored { get; set; }

    [NotMapped]
    public List<PropertyEntry>? TempProperties { get; set; }
}

public abstract class Auditable
{
    public string CreatedBy { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }
}

public enum AuditType
{
    None = 0,
    Create = 1,
    Update = 2,
    Delete = 3,
}
