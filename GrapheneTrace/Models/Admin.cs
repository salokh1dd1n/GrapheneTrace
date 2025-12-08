namespace GrapheneTrace.Models;

public class Admin
{
    public int Id { get; set; }

    // FK to User
    public int UserId { get; set; }

    public string? Notes { get; set; }

    // Navigation back to User
    public User User { get; set; } = default!;
}