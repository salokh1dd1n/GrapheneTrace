namespace GrapheneTrace.Models
{
    public class ClinicianPatient
    {
        public int ClinicianId { get; set; }
        public int PatientId { get; set; }

        public Clinician Clinician { get; set; } = default!;
        public Patient Patient { get; set; } = default!;
    }
}