using GrapheneTrace.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; } = default!;
        public DbSet<Clinician> Clinicians { get; set; } = default!;
        public DbSet<ClinicianPatient> ClinicianPatients { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Patient ↔ User (1:1)
            builder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); // OK

            // Clinician ↔ User (1:1)
            builder.Entity<Clinician>()
                .HasOne(c => c.User)
                .WithOne(u => u.ClinicianProfile)
                .HasForeignKey<Clinician>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); // OK

            // ClinicianPatient (many-to-many) – turn OFF cascade here
            builder.Entity<ClinicianPatient>()
                .HasKey(cp => new { cp.ClinicianId, cp.PatientId });

            builder.Entity<ClinicianPatient>()
                .HasOne(cp => cp.Clinician)
                .WithMany(c => c.PatientLinks)
                .HasForeignKey(cp => cp.ClinicianId)
                .OnDelete(DeleteBehavior.Restrict); // IMPORTANT

            builder.Entity<ClinicianPatient>()
                .HasOne(cp => cp.Patient)
                .WithMany(p => p.ClinicianLinks)
                .HasForeignKey(cp => cp.PatientId)
                .OnDelete(DeleteBehavior.Restrict); // IMPORTANT
        }
    }
}