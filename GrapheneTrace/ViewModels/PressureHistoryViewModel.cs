using System;
using System.Collections.Generic;

namespace GrapheneTrace.ViewModels
{
    public class PressureHistoryViewModel
    {
        public List<PressureHistoryPoint> Points { get; set; } = new();

        public double CurrentPeakPressure { get; set; }          // mmHg
        public double CurrentContactAreaPercent { get; set; }    // %
        public double CurrentRiskScore { get; set; }             // 0–10

        public DateTime? SelectedDate { get; set; }
        public List<DateTime> AvailableDates { get; set; } = new();

        public int[,] LatestFrameMatrix { get; set; } = new int[0, 0];
    }

    public class PressureHistoryPoint
    {
        public DateTime Timestamp { get; set; }
        public double PeakPressure { get; set; }         // mmHg
        public double ContactAreaPercent { get; set; }   // %
        public double RiskScore { get; set; }            // 0–10
    }
}