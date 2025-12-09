using GrapheneTrace.Models;

namespace GrapheneTrace.Services
{
    public class PressureAnalysisService
    {
        private const int LowerThreshold = 10;   // pixels <10 = ignored
        private const int ContactThreshold = 20; // threshold for contact
        
        public FrameMetrics ComputeMetrics(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            int peak = 0;
            int contactPixels = 0;
            int totalPixels = rows * cols;

            long sumPressure = 0;

            long leftSum = 0;
            long rightSum = 0;

            int half = cols / 2;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    int v = matrix[y, x];

                    if (v > peak)
                        peak = v;

                    if (v >= ContactThreshold)
                        contactPixels++;

                    sumPressure += v;

                    if (x < half)
                        leftSum += v;
                    else
                        rightSum += v;
                }
            }

            // Contact area %
            double contactPercent = (double)contactPixels / totalPixels * 100.0;

            // Average pressure
            double avg = (double)sumPressure / totalPixels;

            // Left-right balance (range -1 to +1)
            double balance =
                (leftSum + rightSum) == 0
                ? 0
                : (double)(rightSum - leftSum) / (leftSum + rightSum);

            // Compute risk score
            double risk = ComputeRiskScore(peak, contactPercent);

            return new FrameMetrics
            {
                PeakPressureIndex = peak,
                ContactAreaPercent = contactPercent,
                AveragePressure = avg,
                LeftRightBalance = balance,
            };
        }

        private double ComputeRiskScore(int peakPressure, double contactAreaPercent)
        {
            // Normalise values
            double peakScore = Math.Min(peakPressure / 255.0, 1.0);

            double areaScore = contactAreaPercent / 100.0;

            // Weighted risk formula
            return (peakScore * 0.7) + (areaScore * 0.3);
        }
    }
}