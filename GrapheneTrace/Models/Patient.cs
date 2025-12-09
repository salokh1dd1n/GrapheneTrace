using System;
using System.Collections.Generic;

namespace GrapheneTrace.Models
{
    public class Patient
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        public DateTime? DateOfBirth { get; set; }

        // Many-to-many with clinicians
        public ICollection<ClinicianPatient> ClinicianLinks { get; set; } = new List<ClinicianPatient>();
    }
}