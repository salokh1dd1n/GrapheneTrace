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
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public PatientController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context     = context;
        }

        // GET: /Patient/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.ClinicianLinks)
                .ThenInclude(cp => cp.Clinician)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (patient == null)
                return NotFound("Patient profile not found for this user.");

            var vm = new PatientDashboardViewModel
            {
                Patient    = patient,
                Clinicians = patient.ClinicianLinks
                    .Select(cp => cp.Clinician)
                    .ToList()
            };

            return View(vm); // Views/Patient/Index.cshtml
        }
    }
}