using System.Collections.Generic;
using GrapheneTrace.Models;

namespace GrapheneTrace.ViewModels
{
    public class ClinicianDashboardViewModel
    {
        public Clinician Clinician { get; set; }
        public ApplicationUser User => Clinician.User;

        public List<Patient> Patients { get; set; } = new();
    }
}