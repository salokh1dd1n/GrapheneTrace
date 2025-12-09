// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using System.IO;
// using System.Threading.Tasks;
// using GrapheneTrace.Data;
// using GrapheneTrace.Models;
// using GrapheneTrace.Services;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Newtonsoft.Json;
//
// namespace GrapheneTrace.Controllers
// {
//     [Authorize(Roles = "Patient")]
//     public class PatientPressureController : Controller
//     {
//         private readonly AppDbContext _context;
//         private readonly UserManager<ApplicationUser> _userManager;
//         private readonly PressureAnalysisService _analysis;
//
//         public PatientPressureController(
//             AppDbContext context,
//             UserManager<ApplicationUser> userManager,
//             PressureAnalysisService analysis)
//         {
//             _context = context;
//             _userManager = userManager;
//             _analysis = analysis;
//         }
//
//         // GET: /PatientPressure/Upload
//         public IActionResult Upload()
//         {
//             return View();
//         }
//
//         // POST: /PatientPressure/Upload
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Upload(IFormFile csvFile)
//         {
//             if (csvFile == null || csvFile.Length == 0)
//             {
//                 ModelState.AddModelError(string.Empty, "Please select a CSV file.");
//                 return View();
//             }
//
//             var user = await _userManager.GetUserAsync(User);
//             var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
//
//             if (patient == null)
//             {
//                 ModelState.AddModelError(string.Empty, "No patient profile found.");
//                 return View();
//             }
//
//             using var reader = new StreamReader(csvFile.OpenReadStream());
//             var lines = new List<string>();
//
//             while (!reader.EndOfStream)
//             {
//                 var line = await reader.ReadLineAsync();
//                 if (!string.IsNullOrWhiteSpace(line))
//                     lines.Add(line);
//             }
//
//             // process in blocks of 32 lines (one frame)
//             int frameRowCount = 32;
//             int totalLines = lines.Count;
//             int frameIndex = 0;
//
//             while ((frameIndex + 1) * frameRowCount <= totalLines)
//             {
//                 var frameLines = lines.GetRange(frameIndex * frameRowCount, frameRowCount);
//
//                 int[,] frameData = new int[32, 32];
//
//                 for (int r = 0; r < 32; r++)
//                 {
//                     var parts = frameLines[r].Split(',', StringSplitOptions.TrimEntries);
//                     for (int c = 0; c < 32 && c < parts.Length; c++)
//                     {
//                         if (int.TryParse(parts[c], NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
//                             frameData[r, c] = value;
//                         else
//                             frameData[r, c] = 0;
//                     }
//                 }
//
//                 var result = _analysis.AnalyzeFrame(frameData);
//
//                 // create frame entity
//                 var frame = new PressureFrame
//                 {
//                     PatientId = patient.Id,
//                     Timestamp = DateTime.UtcNow.AddMilliseconds(frameIndex), // or from file metadata
//                     RawDataJson = JsonConvert.SerializeObject(frameData),
//                     FlaggedForReview = result.HasHighPressureRegion
//                 };
//
//                 // create metrics
//                 var metrics = new FrameMetrics
//                 {
//                     Frame = frame,
//                     PeakPressureIndex = result.PeakPressureIndex,
//                     ContactAreaPercent = result.ContactAreaPercent,
//                     AveragePressure = result.AveragePressure
//                 };
//
//                 frame.Metrics = metrics;
//
//                 if (result.HasHighPressureRegion)
//                 {
//                     frame.Alerts.Add(new Alert
//                     {
//                         Severity = AlertSeverity.Warning,
//                         Message = "High pressure region detected in this frame."
//                     });
//                 }
//
//                 _context.PressureFrames.Add(frame);
//                 frameIndex++;
//             }
//
//             await _context.SaveChangesAsync();
//
//             return RedirectToAction("History");
//         }
//
//         // GET: /PatientPressure/History
//         public async Task<IActionResult> History(DateTime? from, DateTime? to)
//         {
//             var user = await _userManager.GetUserAsync(User);
//             var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
//
//             if (patient == null)
//                 return NotFound();
//
//             var query = _context.FrameMetrics
//                 .Include(m => m.Frame)
//                 .Where(m => m.Frame.PatientId == patient.Id);
//
//             if (from.HasValue)
//                 query = query.Where(m => m.Frame.Timestamp >= from.Value.ToUniversalTime());
//             if (to.HasValue)
//                 query = query.Where(m => m.Frame.Timestamp <= to.Value.ToUniversalTime());
//
//             var data = await query
//                 .OrderBy(m => m.Frame.Timestamp)
//                 .ToListAsync();
//
//             return View(data); // You’ll use this to feed graphs with JS
//         }
//
//         // GET: /PatientPressure/Frame/{id}
//         public async Task<IActionResult> Frame(Guid id)
//         {
//             var frame = await _context.PressureFrames
//                 .Include(f => f.Metrics)
//                 .Include(f => f.Comments)
//                     .ThenInclude(c => c.User)
//                 .Include(f => f.Comments)
//                     .ThenInclude(c => c.Replies)
//                         .ThenInclude(r => r.ClinicianUser)
//                 .FirstOrDefaultAsync(f => f.Id == id);
//
//             if (frame == null)
//                 return NotFound();
//
//             return View(frame); // show heatmap + metrics + comments
//         }
//
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> AddComment(Guid frameId, string text)
//         {
//             if (string.IsNullOrWhiteSpace(text))
//                 return RedirectToAction("Frame", new { id = frameId });
//
//             var user = await _userManager.GetUserAsync(User);
//
//             var comment = new UserComment
//             {
//                 FrameId = frameId,
//                 UserId = user.Id,
//                 Text = text
//             };
//
//             _context.UserComments.Add(comment);
//             await _context.SaveChangesAsync();
//
//             return RedirectToAction("Frame", new { id = frameId });
//         }
//     }
// }