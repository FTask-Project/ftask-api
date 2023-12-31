﻿using FTask.Repository.Entity;
using Microsoft.AspNetCore.Identity;

namespace FTask.Repository.Identity;

public class Role : IdentityRole<Guid>
{
    public string CreatedBy { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }
    public IEnumerable<User> Users { get; set; } = new List<User>();

    public bool Deleted { get; set; } = false;
}
