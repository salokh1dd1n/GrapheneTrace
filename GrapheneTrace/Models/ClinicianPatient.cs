using System;

namespace GrapheneTrace.Models
{
    public class ClinicianPatient
    {
        public Guid ClinicianId { get; set; }
        public Clinician Clinician { get; set; } = default!;

        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = default!;
    }
}