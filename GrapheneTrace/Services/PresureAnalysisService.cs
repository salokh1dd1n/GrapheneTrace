using System;

namespace GrapheneTrace.Services
{
    public class PressureAnalysisOptions
    {
        public int LowerThreshold { get; set; } = 20;      // for contact area
        public int HighPressureThreshold { get; set; } = 220;  // for alerts
    }

    public class PressureAnalysisResult
    {
        public int PeakPressureIndex { get; set; }
        public double ContactAreaPercent { get; set; }
        public double AveragePressure { get; set; }
        public bool HasHighPressureRegion { get; set; }
    }

    public class PressureAnalysisService
    {
        private readonly PressureAnalysisOptions _options;

        public PressureAnalysisService(PressureAnalysisOptions options)
        {
            _options = options;
        }

        public PressureAnalysisResult AnalyzeFrame(int[,] rawFrame)
        {
            int rows = rawFrame.GetLength(0);
            int cols = rawFrame.GetLength(1);
            int[,] frame = new int[rows, cols];

            int totalCount = rows * cols;
            int contactCount = 0;
            int peak = 0;
            long sum = 0;
            bool highRegion = false;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int v = rawFrame[r, c];

                    // clamp sensor noise
                    if (v < 0) v = 0;
                    if (v > 255) v = 255;

                    frame[r, c] = v;
                    sum += v;

                    if (v >= _options.LowerThreshold)
                        contactCount++;

                    if (v > peak)
                        peak = v;

                    if (v >= _options.HighPressureThreshold)
                        highRegion = true;
                }
            }

            double contactAreaPercent = contactCount / (double)totalCount * 100.0;
            double avg = sum / (double)totalCount;

            return new PressureAnalysisResult
            {
                PeakPressureIndex = peak,
                ContactAreaPercent = contactAreaPercent,
                AveragePressure = avg,
                HasHighPressureRegion = highRegion
            };
        }
    }
}