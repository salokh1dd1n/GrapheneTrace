using System.Linq;
using System.Threading.Tasks;
using GrapheneTrace.Data;
using GrapheneTrace.Models;
using GrapheneTrace.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    [Authorize(Roles = "Clinician")]
    public class ClinicianController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public ClinicianController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /Clinician/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var clinician = await _context.Clinicians
                .Include(c => c.User)
                .Include(c => c.PatientLinks)
                .ThenInclude(cp => cp.Patient)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (clinician == null)
                return NotFound("Clinician profile not found for this user.");

            // If AccessAllPatients == true -> see all patients.
            // Otherwise -> only patients linked via ClinicianPatients.
            var patientsQuery = _context.Patients
                .Include(p => p.User)
                .AsQueryable();

            var patients = clinician.AccessAllPatients
                ? await patientsQuery.ToListAsync()
                : clinician.PatientLinks
                    .Select(cp => cp.Patient)
                    .ToList();

            var vm = new ClinicianDashboardViewModel
            {
                Clinician = clinician,
                Patients = patients
            };

            return View(vm); // Views/Clinician/Index.cshtml
        }

        // OPTIONAL: view specific patient details (ONLY own patient)
        [HttpGet]
        public async Task<IActionResult> PatientDetails(System.Guid id)
        {
            var user = await _userManager.GetUserAsync(User);

            var clinician = await _context.Clinicians
                .Include(c => c.PatientLinks)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (clinician == null)
                return NotFound();

            // Check access
            bool canSee = clinician.AccessAllPatients ||
                          clinician.PatientLinks.Any(cp => cp.PatientId == id);

            if (!canSee)
                return Forbid();

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            return View(patient); // Views/Clinician/PatientDetails.cshtml
        }

        // GET: /Clinician/Alerts
        public async Task<IActionResult> Alerts()
        {
            var clinicianUser = await _userManager.GetUserAsync(User);

            var clinician = await _context.Clinicians
                .Include(c => c.PatientLinks)
                .ThenInclude(cp => cp.Patient)
                .FirstOrDefaultAsync(c => c.UserId == clinicianUser.Id);

            if (clinician == null)
                return NotFound("Clinician profile not found.");

            IQueryable<Alert> alertsQuery = _context.Alerts
                .Include(a => a.Frame)
                .ThenInclude(f => f.Patient)
                .ThenInclude(p => p.User)
                .Where(a => a.Status == AlertStatus.New);

            // If this clinician can only see their linked patients:
            if (!clinician.AccessAllPatients)
            {
                var patientIds = clinician.PatientLinks.Select(pl => pl.PatientId).ToList();
                alertsQuery = alertsQuery.Where(a => patientIds.Contains(a.Frame.PatientId));
            }

            var alerts = await alertsQuery
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(alerts); // list of alerts with link to /Clinician/Frame/{id}
        }

        // GET: /Clinician/Frame/{id}
        public async Task<IActionResult> Frame(Guid id)
        {
            var frame = await _context.PressureFrames
                .Include(f => f.Patient).ThenInclude(p => p.User)
                .Include(f => f.Metrics)
                .Include(f => f.Comments)
                .ThenInclude(c => c.User)
                .Include(f => f.Comments)
                .ThenInclude(c => c.Replies)
                .ThenInclude(r => r.ClinicianUser)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (frame == null)
                return NotFound();

            return View(frame); // similar to Patient view but with reply form
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyToComment(Guid commentId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction("Alerts");

            var clinicianUser = await _userManager.GetUserAsync(User);

            var reply = new ClinicianReply
            {
                CommentId = commentId,
                ClinicianUserId = clinicianUser.Id,
                Text = text
            };

            _context.ClinicianReplies.Add(reply);
            await _context.SaveChangesAsync();

            var comment = await _context.UserComments
                .Include(c => c.Frame)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            return RedirectToAction("Frame", new { id = comment!.FrameId });
        }
        
        // Patient Frames
        public async Task<IActionResult> PatientFrames(Guid patientId)
        {
            var user = await _userManager.GetUserAsync(User);
            var clinician = await _context.Clinicians
                .Include(c => c.PatientLinks)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // ensure this patient belongs to this clinician (if not AccessAllPatients)
            if (!clinician.AccessAllPatients &&
                !clinician.PatientLinks.Any(pl => pl.PatientId == patientId))
            {
                return Forbid();
            }

            var frames = await _context.PressureFrames
                .Include(f => f.Metrics)
                .Where(f => f.PatientId == patientId)
                .OrderByDescending(f => f.Timestamp)
                .ToListAsync();

            return View(frames);
        }
    }
}