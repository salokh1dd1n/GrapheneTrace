namespace GrapheneTrace.ViewModels;

public class AdminFormViewModel
{
    public int? Id { get; set; }   // Admin Id (null for create)

    // User fields
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;  // plain for now
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Admin-specific
    public string? Notes { get; set; }
}