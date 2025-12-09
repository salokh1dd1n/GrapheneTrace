namespace GrapheneTrace.Models;

public class PressureFrame
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Which patient is this for?
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = default!;

    // When this frame was recorded (or derived from file time)
    public DateTime Timestamp { get; set; }

    // Raw 32x32 data stored as JSON (simplest)
    public string RawDataJson { get; set; } = default!;

    // Mark frames that need clinician review
    public bool FlaggedForReview { get; set; }

    // Navigation
    public FrameMetrics Metrics { get; set; } = default!;
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<UserComment> Comments { get; set; } = new List<UserComment>();
}