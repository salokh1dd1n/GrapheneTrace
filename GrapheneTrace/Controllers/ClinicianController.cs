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
            _context     = context;
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
                Patients  = patients
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
    }
}