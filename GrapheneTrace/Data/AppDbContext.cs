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

        // Pressure Data Models
        public DbSet<PressureFrame> PressureFrames { get; set; } = default!;
        public DbSet<FrameMetrics> FrameMetrics { get; set; } = default!;
        public DbSet<Alert> Alerts { get; set; } = default!;
        public DbSet<UserComment> UserComments { get; set; } = default!;
        public DbSet<ClinicianReply> ClinicianReplies { get; set; } = default!;

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
            // ===============================
            // PressureFrame ↔ Patient
            // ===============================
            builder.Entity<PressureFrame>()
                .HasOne(f => f.Patient)
                .WithMany() // or .WithMany(p => p.PressureFrames) if you add a collection
                .HasForeignKey(f => f.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===============================
            // PressureFrame ↔ FrameMetrics
            // ===============================
            builder.Entity<PressureFrame>()
                .HasOne(f => f.Metrics)
                .WithOne(m => m.Frame)
                .HasForeignKey<FrameMetrics>(m => m.FrameId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===============================
            // PressureFrame ↔ UserComments
            // ===============================
            builder.Entity<PressureFrame>()
                .HasMany(f => f.Comments)
                .WithOne(c => c.Frame)
                .HasForeignKey(c => c.FrameId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===============================
            // PressureFrame ↔ Alerts
            // ===============================
            builder.Entity<PressureFrame>()
                .HasMany(f => f.Alerts)
                .WithOne(a => a.Frame)
                .HasForeignKey(a => a.FrameId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===============================
            // UserComment ↔ ClinicianReply
            // ===============================
            builder.Entity<UserComment>()
                .HasMany(c => c.Replies)
                .WithOne(r => r.Comment)
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===============================
            // UserComment ↔ ApplicationUser
            // (author of the comment)
            // ===============================
            builder.Entity<UserComment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===============================
            // ClinicianReply ↔ ApplicationUser
            // (clinician who replied)
            // ===============================
            builder.Entity<ClinicianReply>()
                .HasOne(r => r.ClinicianUser)
                .WithMany()
                .HasForeignKey(r => r.ClinicianUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}