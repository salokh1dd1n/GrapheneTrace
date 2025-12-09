namespace GrapheneTrace.Models
{
    public class FrameMetrics
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid FrameId { get; set; }
        public PressureFrame Frame { get; set; } = default!;

        public int PeakPressureIndex { get; set; }
        public double ContactAreaPercent { get; set; }

        // "Nice to have" extra metrics
        public double AveragePressure { get; set; }
        public double LeftRightBalance { get; set; }  // -1 = all left, +1 = all right
    }
}