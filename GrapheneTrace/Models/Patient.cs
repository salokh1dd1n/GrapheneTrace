namespace GrapheneTrace.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public User User { get; set; } = default!;

        // Which clinicians have explicit access to this patient
        public ICollection<ClinicianPatient> ClinicianLinks { get; set; } = new List<ClinicianPatient>();
    }
}