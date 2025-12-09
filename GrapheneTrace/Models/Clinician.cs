using System;
using System.Collections.Generic;

namespace GrapheneTrace.Models
{
    public class Clinician
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        public string? Specialisation { get; set; }
        public string? RegistrationNumber { get; set; }

        // If true → clinician can access ALL patients. If false → only those in links.
        public bool AccessAllPatients { get; set; } = true;

        public ICollection<ClinicianPatient> PatientLinks { get; set; } = new List<ClinicianPatient>();
    }
}