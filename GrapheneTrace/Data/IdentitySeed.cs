using GrapheneTrace.Models;
using Microsoft.AspNetCore.Identity;

namespace GrapheneTrace.Data
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<AppDbContext>();

            // --- Create Roles ---
            string[] roles = { "Admin", "Clinician", "Patient" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ---------------------------------------------------------
            // SEED 1) ADMIN
            // ---------------------------------------------------------
            string adminEmail = "admin@example.com";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Admin"
                };

                var create = await userManager.CreateAsync(adminUser, "Admin123!");
                if (create.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // ---------------------------------------------------------
            // SEED 2) CLINICIAN
            // ---------------------------------------------------------
            string clinicianEmail = "clinician@example.com";

            var clinicianUser = await userManager.FindByEmailAsync(clinicianEmail);
            if (clinicianUser == null)
            {
                clinicianUser = new ApplicationUser
                {
                    UserName = clinicianEmail,
                    Email = clinicianEmail,
                    FirstName = "John",
                    LastName = "Clinician"
                };

                var create = await userManager.CreateAsync(clinicianUser, "Clinician123!");
                if (create.Succeeded)
                {
                    await userManager.AddToRoleAsync(clinicianUser, "Clinician");

                    // Add profile row
                    var clinicianProfile = new Clinician
                    {
                        UserId = clinicianUser.Id,
                        Specialisation = "General Medicine",
                        RegistrationNumber = "CLN-001",
                        AccessAllPatients = true
                    };

                    context.Clinicians.Add(clinicianProfile);
                    await context.SaveChangesAsync();
                }
            }

            // ---------------------------------------------------------
            // SEED 3) PATIENT
            // ---------------------------------------------------------
            string patientEmail = "patient@example.com";

            var patientUser = await userManager.FindByEmailAsync(patientEmail);
            if (patientUser == null)
            {
                patientUser = new ApplicationUser
                {
                    UserName = patientEmail,
                    Email = patientEmail,
                    FirstName = "Alice",
                    LastName = "Patient"
                };

                var create = await userManager.CreateAsync(patientUser, "Patient123!");
                if (create.Succeeded)
                {
                    await userManager.AddToRoleAsync(patientUser, "Patient");

                    // Add patient profile row
                    var patientProfile = new Patient
                    {
                        UserId = patientUser.Id,
                        DateOfBirth = new DateTime(2000, 1, 1)
                    };

                    context.Patients.Add(patientProfile);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}