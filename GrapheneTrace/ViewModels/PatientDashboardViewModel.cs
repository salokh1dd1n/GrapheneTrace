using GrapheneTrace.Models;

namespace GrapheneTrace.ViewModels
{
    public class PatientDashboardViewModel
    {
        public Patient Patient { get; set; }
        public ApplicationUser User => Patient.User;

        // Clinicians assigned to this patient
        public List<Clinician> Clinicians { get; set; } = new();
    }
}