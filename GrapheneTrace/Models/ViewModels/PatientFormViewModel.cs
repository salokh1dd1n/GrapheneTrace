namespace GrapheneTrace.Models.ViewModels
{
    public class PatientFormViewModel
    {
        public int? Id { get; set; }   // Patient Id (null for create)

        // User fields
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // plain for now
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Patient fields
        public DateTime? DateOfBirth { get; set; }
    }
}