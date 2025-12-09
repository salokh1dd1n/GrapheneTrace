using System;
using System.Collections.Generic;

namespace GrapheneTrace.Models
{
    public class PressureFrame
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = default!;

        public DateTime Timestamp { get; set; }

        // 32x32 matrix as JSON
        public string MatrixJson { get; set; } = default!;

        public bool FlaggedForReview { get; set; } = false;

        public FrameMetrics Metrics { get; set; } = default!;
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
        public ICollection<UserComment> Comments { get; set; } = new List<UserComment>();
    }
}