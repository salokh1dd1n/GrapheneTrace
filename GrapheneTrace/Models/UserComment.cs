namespace GrapheneTrace.Models
{
    public class UserComment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid FrameId { get; set; }
        public PressureFrame Frame { get; set; } = default!;

        public string UserId { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Text { get; set; } = default!;

        public ICollection<ClinicianReply> Replies { get; set; } = new List<ClinicianReply>();
    }
}