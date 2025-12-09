using Microsoft.AspNetCore.Identity;

namespace GrapheneTrace.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName  { get; set; }

        // Optional navigation
        public Patient?   PatientProfile   { get; set; }
        public Clinician? ClinicianProfile { get; set; }
    }
}