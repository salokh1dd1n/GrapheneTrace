using Microsoft.AspNetCore.Mvc.Rendering;

namespace GrapheneTrace.Models.ViewModels
{
    public class ClinicianFormViewModel
    {
        public int? Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Specialisation { get; set; }
        public string? RegistrationNumber { get; set; }
        public bool AccessAllPatients { get; set; } = true;
        public List<int> SelectedPatientIds { get; set; } = new();
        public List<SelectListItem> PatientsSelectList { get; set; } = new();
    }
}