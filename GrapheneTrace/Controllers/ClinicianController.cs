using System.Linq;
using System.Threading.Tasks;
using GrapheneTrace.Data;
using GrapheneTrace.Models;
using GrapheneTrace.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    public class CliniciansController : Controller
    {
        private readonly AppDbContext _context;

        public CliniciansController(AppDbContext context)
        {
            _context = context;
        }

        // Helper: build list of patients for multi-select
        private async Task<List<SelectListItem>> BuildPatientsSelectListAsync(List<int>? selectedIds = null)
        {
            selectedIds ??= new List<int>();

            var patients = await _context.Patients
                .Include(p => p.User)
                .ToListAsync();

            return patients
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.User.FirstName} {p.User.LastName} ({p.User.Email})",
                    Selected = selectedIds.Contains(p.Id)
                })
                .ToList();
        }

        // GET: Clinicians
        public async Task<IActionResult> Index()
        {
            var clinicians = await _context.Clinicians
                .Include(c => c.User)
                .Include(c => c.PatientLinks)
                    .ThenInclude(cp => cp.Patient)
                        .ThenInclude(p => p.User)
                .ToListAsync();

            return View(clinicians);
        }

        // GET: Clinicians/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var clinician = await _context.Clinicians
                .Include(c => c.User)
                .Include(c => c.PatientLinks)
                    .ThenInclude(cp => cp.Patient)
                        .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinician == null)
                return NotFound();

            return View(clinician);
        }

        // GET: Clinicians/Create
        public async Task<IActionResult> Create()
        {
            var vm = new ClinicianFormViewModel
            {
                AccessAllPatients = true,
                PatientsSelectList = await BuildPatientsSelectListAsync()
            };

            return View(vm);
        }

        // POST: Clinicians/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClinicianFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.PatientsSelectList = await BuildPatientsSelectListAsync(model.SelectedPatientIds);
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = model.Password,   // TODO: hash later
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Role = UserRole.Clinician
            };

            var clinician = new Clinician
            {
                User = user,
                Specialisation = model.Specialisation,
                RegistrationNumber = model.RegistrationNumber,
                AccessAllPatients = model.AccessAllPatients
            };

            // If not global access, add selected patient links
            if (!model.AccessAllPatients && model.SelectedPatientIds.Any())
            {
                foreach (var patientId in model.SelectedPatientIds.Distinct())
                {
                    clinician.PatientLinks.Add(new ClinicianPatient
                    {
                        PatientId = patientId,
                        Clinician = clinician
                    });
                }
            }

            _context.Clinicians.Add(clinician);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Clinicians/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var clinician = await _context.Clinicians
                .Include(c => c.User)
                .Include(c => c.PatientLinks)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinician == null)
                return NotFound();

            var selectedIds = clinician.PatientLinks.Select(cp => cp.PatientId).ToList();

            var vm = new ClinicianFormViewModel
            {
                Id = clinician.Id,
                Username = clinician.User.Username,
                // Password left empty intentionally
                FirstName = clinician.User.FirstName,
                LastName = clinician.User.LastName,
                Email = clinician.User.Email,
                Specialisation = clinician.Specialisation,
                RegistrationNumber = clinician.RegistrationNumber,
                AccessAllPatients = clinician.AccessAllPatients,
                SelectedPatientIds = selectedIds,
                PatientsSelectList = await BuildPatientsSelectListAsync(selectedIds)
            };

            return View(vm);
        }

        // POST: Clinicians/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClinicianFormViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.PatientsSelectList = await BuildPatientsSelectListAsync(model.SelectedPatientIds);
                return View(model);
            }

            var clinician = await _context.Clinicians
                .Include(c => c.User)
                .Include(c => c.PatientLinks)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinician == null)
                return NotFound();

            // Update User
            clinician.User.Username = model.Username;
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                clinician.User.PasswordHash = model.Password;  // TODO: hash
            }
            clinician.User.FirstName = model.FirstName;
            clinician.User.LastName = model.LastName;
            clinician.User.Email = model.Email;

            // Update Clinician
            clinician.Specialisation = model.Specialisation;
            clinician.RegistrationNumber = model.RegistrationNumber;
            clinician.AccessAllPatients = model.AccessAllPatients;

            // Clear old links
            var existingLinks = _context.ClinicianPatients
                .Where(cp => cp.ClinicianId == clinician.Id);
            _context.ClinicianPatients.RemoveRange(existingLinks);

            // Re-add selected links if not global access
            if (!model.AccessAllPatients && model.SelectedPatientIds.Any())
            {
                foreach (var pid in model.SelectedPatientIds.Distinct())
                {
                    _context.ClinicianPatients.Add(new ClinicianPatient
                    {
                        ClinicianId = clinician.Id,
                        PatientId = pid
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Clinicians/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var clinician = await _context.Clinicians
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinician == null)
                return NotFound();

            return View(clinician);
        }

        // POST: Clinicians/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clinician = await _context.Clinicians
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinician == null)
                return NotFound();

            _context.Users.Remove(clinician.User); // cascades to Clinician (and links if configured)
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}