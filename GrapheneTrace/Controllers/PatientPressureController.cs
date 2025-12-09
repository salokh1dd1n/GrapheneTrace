using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using GrapheneTrace.Data;
using GrapheneTrace.Models;
using GrapheneTrace.Services;
using GrapheneTrace.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GrapheneTrace.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientPressureController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly PressureAnalysisService _pressureAnalysisService;

        // Adjust if sensor calibration is different
        private const double MaxSensorPressureMmHg = 200.0;

        public PatientPressureController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            PressureAnalysisService pressureAnalysisService)
        {
            _context = context;
            _userManager = userManager;
            _pressureAnalysisService = pressureAnalysisService;
        }

        // GET: /PatientPressure/Upload
        public IActionResult Upload()
        {
            return View();
        }

        // ========== Helper: CSV → List<int[,]> ==========
        private async Task<List<int[,]>> ParseCsvToMatricesAsync(IFormFile file)
        {
            var lines = new List<string>();

            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line))
                        lines.Add(line);
                }
            }

            const int rowsPerFrame = 32;
            const int colsPerFrame = 32;

            var matrices = new List<int[,]>();

            for (int i = 0; i + rowsPerFrame <= lines.Count; i += rowsPerFrame)
            {
                var matrix = new int[rowsPerFrame, colsPerFrame];

                for (int row = 0; row < rowsPerFrame; row++)
                {
                    var parts = lines[i + row]
                        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    for (int col = 0; col < colsPerFrame && col < parts.Length; col++)
                    {
                        if (int.TryParse(parts[col], out int value))
                            matrix[row, col] = value;
                    }
                }

                matrices.Add(matrix);
            }

            return matrices;
        }

// ========== Helper: 2D → Jagged JSON ==========
        private static int[][] ToJagged(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var result = new int[rows][];

            for (int y = 0; y < rows; y++)
            {
                result[y] = new int[cols];
                for (int x = 0; x < cols; x++)
                    result[y][x] = matrix[y, x];
            }

            return result;
        }

        // POST: /PatientPressure/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(PressureUploadViewModel model)
        {
            if (!ModelState.IsValid || model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a valid CSV file.");
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
                return NotFound("Patient profile not found.");

            // 1) Parse CSV → list of 32×32 matrices
            var matrices = await ParseCsvToMatricesAsync(model.File);

            if (matrices.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No valid frames found in CSV.");
                return View(model);
            }

            // 2) Spread all frames evenly across the chosen day
            int frameCount = matrices.Count;
            const double SecondsPerDay = 86400.0;
            double secondsPerFrame = SecondsPerDay / frameCount;

            DateTime startOfDay = model.Day.Date;

            int frameIndex = 0;
            foreach (var matrix in matrices)
            {
                DateTime frameTimestamp = startOfDay.AddSeconds(frameIndex * secondsPerFrame);

                var frame = new PressureFrame
                {
                    Id = Guid.NewGuid(),
                    PatientId = patient.Id,
                    Timestamp = frameTimestamp,
                    MatrixJson = JsonConvert.SerializeObject(ToJagged(matrix)),
                    FlaggedForReview = false
                };

                // Compute metrics for this frame
                var metrics = _pressureAnalysisService.ComputeMetrics(matrix);
                metrics.FrameId = frame.Id;
                frame.Timestamp = frameTimestamp;

                frame.Metrics = metrics;

                // Simple auto-flag rule (you can tune these thresholds)
                if (metrics.PeakPressureIndex > 220 || metrics.ContactAreaPercent > 70)
                {
                    frame.FlaggedForReview = true;
                }

                _context.PressureFrames.Add(frame);
                frameIndex++;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("History");
        }


        // =====================================================
        // HISTORY: /PatientPressure/History?range=1h|6h|24h
        // PeakPressure -> mmHg
        // ContactArea  -> 0–100%
        // RiskScore    -> 0–10
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> History(string range = "24h", DateTime? selectedDate = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
                return NotFound("Patient profile not found.");

            // ------------ 1) Decide time window ------------
            TimeSpan window;
            switch (range)
            {
                case "1h":
                    window = TimeSpan.FromHours(1);
                    break;
                case "6h":
                    window = TimeSpan.FromHours(6);
                    break;
                case "24h":
                default:
                    range = "24h";
                    window = TimeSpan.FromHours(24);
                    break;
            }

            // ------------ 2) Decide end time ------------
            DateTime endTimeUtc;
            if (selectedDate.HasValue)
            {
                // "End of that day" (simple version)
                var dayLocal = selectedDate.Value.Date;
                endTimeUtc = dayLocal.AddDays(1); // 00:00 next day
            }
            else
            {
                endTimeUtc = DateTime.UtcNow;
            }

            var fromTimeUtc = endTimeUtc - window;

            // ------------ 3) Base query with metrics ------------
            var framesQuery = _context.PressureFrames
                .Include(f => f.Metrics)
                .Where(f => f.PatientId == patient.Id && f.Metrics != null);

            // ------------ 4) Filter by time window ------------
            var framesInRange = await framesQuery
                .Where(f => f.Timestamp >= fromTimeUtc && f.Timestamp <= endTimeUtc)
                .OrderBy(f => f.Timestamp)
                .ToListAsync();

            // If no data in this window, but some data exists at all, fall back to all data
            if (!framesInRange.Any())
            {
                framesInRange = await framesQuery
                    .OrderBy(f => f.Timestamp)
                    .ToListAsync();

                if (!framesInRange.Any())
                {
                    // No data at all
                    ViewBag.SelectedRange = range;
                    return View(new PressureHistoryViewModel());
                }
            }

            var vm = new PressureHistoryViewModel();

            foreach (var frame in framesInRange)
            {
                var m = frame.Metrics!;
                int rawPeak = m.PeakPressureIndex; // 0–255
                double peakMmHg = rawPeak / 255.0 * MaxSensorPressureMmHg;

                double contact = m.ContactAreaPercent;
                if (contact < 0) contact = 0;
                if (contact > 100) contact = 100;

                double risk10 = ComputeRiskScoreOutOf10(rawPeak, contact);

                vm.Points.Add(new PressureHistoryPoint
                {
                    Timestamp = frame.Timestamp,
                    PeakPressure = peakMmHg,
                    ContactAreaPercent = contact,
                    RiskScore = risk10
                });
            }

            if (vm.Points.Any())
            {
                // Use *max* peak over window for the KPI
                vm.CurrentPeakPressure = vm.Points.Max(p => p.PeakPressure);
                vm.CurrentContactAreaPercent = vm.Points.Average(p => p.ContactAreaPercent);
                vm.CurrentRiskScore = vm.Points.Max(p => p.RiskScore);

                // Selected date: keep what user chose, else use last point date
                vm.SelectedDate = selectedDate?.Date ?? vm.Points.Last().Timestamp.Date;

                vm.AvailableDates = vm.Points
                    .Select(p => p.Timestamp.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                // Latest frame for heatmap
                var lastFrame = framesInRange.Last();
                vm.LatestFrameMatrix = DecodeMatrixJson(lastFrame.MatrixJson);
            }

            ViewBag.SelectedRange = range;

            return View(vm);
        }

        // Same helper you already had
        private double ComputeRiskScoreOutOf10(int rawPeakIndex, double contactAreaPercent)
        {
            double peakScore = rawPeakIndex / 255.0 * 10.0; // 0–10
            double areaScore = contactAreaPercent / 100.0 * 10.0; // 0–10
            double combined = 0.6 * peakScore + 0.4 * areaScore;
            if (combined < 0) combined = 0;
            if (combined > 10) combined = 10;
            return combined;
        }

        private int[,] DecodeMatrixJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new int[0, 0];

            var jagged = Newtonsoft.Json.JsonConvert
                .DeserializeObject<int[][]>(json);

            if (jagged == null || jagged.Length == 0)
                return new int[0, 0];

            int rows = jagged.Length;
            int cols = jagged[0].Length;
            var result = new int[rows, cols];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    result[y, x] = jagged[y][x];
                }
            }

            return result;
        }
        
        // =====================================================
        // KEEP your existing Upload actions here
        // (GET + POST for CSV upload) – no need to change them
        // =====================================================

        // GET: /PatientPressure/Frame/{id}
        public async Task<IActionResult> Frame(Guid id)
        {
            var frame = await _context.PressureFrames
                .Include(f => f.Metrics)
                .Include(f => f.Comments)
                .ThenInclude(c => c.User)
                .Include(f => f.Comments)
                .ThenInclude(c => c.Replies)
                .ThenInclude(r => r.ClinicianUser)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (frame == null)
                return NotFound();

            return View(frame); // show heatmap + metrics + comments
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Guid frameId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction("Frame", new { id = frameId });

            var user = await _userManager.GetUserAsync(User);

            var comment = new UserComment
            {
                FrameId = frameId,
                UserId = user.Id,
                Text = text
            };

            _context.UserComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Frame", new { id = frameId });
        }
    }
}