namespace GrapheneTrace.Models
{
    public class ClinicianReply
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CommentId { get; set; }
        public UserComment Comment { get; set; } = default!;

        public string ClinicianUserId { get; set; } = default!;
        public ApplicationUser ClinicianUser { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Text { get; set; } = default!;
    }
}