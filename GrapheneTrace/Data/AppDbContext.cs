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
    public DbSet<ClinicianPatient> ClinicianPatients { get; set; } = default!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // ---------- USER ----------
    modelBuilder.Entity<User>(entity =>
    {
        entity.HasKey(u => u.Id);

        entity.Property(u => u.Username)
              .IsRequired()
              .HasMaxLength(100);

        entity.Property(u => u.PasswordHash)
              .IsRequired();

        entity.Property(u => u.FirstName)
              .IsRequired()
              .HasMaxLength(100);

        entity.Property(u => u.LastName)
              .IsRequired()
              .HasMaxLength(100);

        entity.Property(u => u.Email)
              .IsRequired()
              .HasMaxLength(200);

        entity.Property(u => u.Role)
              .IsRequired();
    });

    // ---------- ADMIN ----------
    modelBuilder.Entity<Admin>(entity =>
    {
        entity.HasKey(a => a.Id);

        entity.Property(a => a.Notes)
              .HasMaxLength(500);

        entity.HasOne(a => a.User)
              .WithOne(u => u.AdminProfile)
              .HasForeignKey<Admin>(a => a.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    });

    // ---------- PATIENT ----------
    modelBuilder.Entity<Patient>(entity =>
    {
        entity.HasKey(p => p.Id);

        entity.Property(p => p.DateOfBirth);

        entity.HasOne(p => p.User)
              .WithOne(u => u.PatientProfile)
              .HasForeignKey<Patient>(p => p.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    });

    // ---------- CLINICIAN ----------
    modelBuilder.Entity<Clinician>(entity =>
    {
        entity.HasKey(c => c.Id);

        entity.Property(c => c.Specialisation)
              .HasMaxLength(200);

        entity.Property(c => c.RegistrationNumber)
              .HasMaxLength(100);

        entity.HasOne(c => c.User)
              .WithOne(u => u.ClinicianProfile)
              .HasForeignKey<Clinician>(c => c.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    });

    // ---------- CLINICIAN–PATIENT (many-to-many) ----------
    modelBuilder.Entity<ClinicianPatient>(entity =>
    {
          entity.HasKey(cp => new { cp.ClinicianId, cp.PatientId });

          entity.HasOne(cp => cp.Clinician)
                .WithMany(c => c.PatientLinks)
                .HasForeignKey(cp => cp.ClinicianId)
                .OnDelete(DeleteBehavior.Restrict);   

          entity.HasOne(cp => cp.Patient)
                .WithMany(p => p.ClinicianLinks)
                .HasForeignKey(cp => cp.PatientId)
                .OnDelete(DeleteBehavior.Restrict);   
    });
}
}