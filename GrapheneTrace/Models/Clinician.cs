using System.Collections.Generic;

namespace GrapheneTrace.Models
{
    public class Clinician
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string? Specialisation { get; set; }
        public string? RegistrationNumber { get; set; }

        // If true → clinician can see ALL patients
        public bool AccessAllPatients { get; set; } = true;

        public User User { get; set; } = default!;

        // 🔹 Navigation: patients this clinician can access (when AccessAllPatients == false)
        public ICollection<ClinicianPatient> PatientLinks { get; set; } = new List<ClinicianPatient>();
    }
}