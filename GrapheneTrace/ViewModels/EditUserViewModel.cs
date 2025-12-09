using System;
using System.ComponentModel.DataAnnotations;

namespace GrapheneTrace.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        public string Id { get; set; }  // ApplicationUser Id

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        public string? Role { get; set; }   // "Admin", "Clinician", "Patient"

        // PATIENT-ONLY
        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime? DateOfBirth { get; set; }

        // CLINICIAN-ONLY
        [Display(Name = "Specialisation")]
        public string? Specialisation { get; set; }

        [Display(Name = "Registration number")]
        public string? RegistrationNumber { get; set; }

        [Display(Name = "Access all patients")]
        public bool? AccessAllPatients { get; set; }
    }
}