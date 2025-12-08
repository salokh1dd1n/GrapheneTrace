namespace GrapheneTrace.Models;

public class Clinician
{
    public int Id { get; set; }

    // FK to User table
    public int UserId { get; set; }

    public string? Specialisation { get; set; }
    public string? RegistrationNumber { get; set; }

    // Navigation
    public User User { get; set; }
}