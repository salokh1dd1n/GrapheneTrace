using System;

namespace GrapheneTrace.Models
{
    public class FrameMetrics
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid FrameId { get; set; }
        public PressureFrame Frame { get; set; } = default!;

        public DateTime Timestamp { get; set; }

        public int PeakPressureIndex { get; set; }
        public double ContactAreaPercent { get; set; }

        public double AveragePressure { get; set; }
        public double LeftRightBalance { get; set; }
        public double RiskScore { get; set; }
    }
}