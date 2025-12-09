namespace GrapheneTrace.Models
{
    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }

    public enum AlertStatus
    {
        New,
        Reviewed,
        Resolved
    }

    public class Alert
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid FrameId { get; set; }
        public PressureFrame Frame { get; set; } = default!;

        public AlertSeverity Severity { get; set; }
        public AlertStatus Status { get; set; } = AlertStatus.New;

        public string Message { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}