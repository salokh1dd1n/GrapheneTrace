using GrapheneTrace.Data;
using GrapheneTrace.Models;
using GrapheneTrace.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    public class PatientsController : Controller
    {
        private readonly AppDbContext _context;

        public PatientsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .ToListAsync();

            return View(patients);
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            return View(new PatientFormViewModel());
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // create User
            var user = new User
            {
                Username = model.Username,
                PasswordHash = model.Password,   // TODO: hash later
                FullName = model.FullName,
                Email = model.Email,
                Role = UserRole.Patient
            };

            var patient = new Patient
            {
                User = user,
                DateOfBirth = model.DateOfBirth,
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            var vm = new PatientFormViewModel
            {
                Id = patient.Id,
                Username = patient.User.Username,
                // we DON'T load password back – leave empty
                FullName = patient.User.FullName,
                Email = patient.User.Email,
                DateOfBirth = patient.DateOfBirth,
            };

            return View(vm);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            // update user
            patient.User.Username = model.Username;
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                patient.User.PasswordHash = model.Password;  // TODO: hash later
            }
            patient.User.FullName = model.FullName;
            patient.User.Email = model.Email;

            // update patient fields
            patient.DateOfBirth = model.DateOfBirth;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            // safer: delete the User → cascade deletes Patient (if configured)
            _context.Users.Remove(patient.User);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}