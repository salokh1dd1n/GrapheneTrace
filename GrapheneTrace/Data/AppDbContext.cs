using GrapheneTrace.Models;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    // connect the C# model User to table in the DB
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Admin> Admins { get; set; } = default!;
    public DbSet<Clinician> Clinicians { get; set; } = default!;
    public DbSet<Patient> Patients { get; set; } = default!;
}