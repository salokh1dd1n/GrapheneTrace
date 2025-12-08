namespace GrapheneTrace.Models;

public class User
{
    public int Id { get; set; }

    // Login
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Personal Info
    
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    // Navigation
    // public Patient? PatientProfile { get; set; }
    // public Clinician? ClinicianProfile { get; set; }
    public Admin? AdminProfile { get; set; }
}