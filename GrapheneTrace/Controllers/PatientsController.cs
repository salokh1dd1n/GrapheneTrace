using System.Threading.Tasks;
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

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            var vm = new PatientFormViewModel();
            return View(vm);
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Create linked User
            var user = new User
            {
                Username = model.Username,
                PasswordHash = model.Password, // TODO: hash later
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Role = UserRole.Patient
            };

            // Create Patient
            var patient = new Patient
            {
                User = user,
                DateOfBirth = model.DateOfBirth
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

            if (patient == null)
                return NotFound();

            var vm = new PatientFormViewModel
            {
                Id = patient.Id,
                Username = patient.User.Username,
                // Password left empty on purpose
                FirstName = patient.User.FirstName,
                LastName = patient.User.LastName,
                Email = patient.User.Email,
                DateOfBirth = patient.DateOfBirth
            };

            return View(vm);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientFormViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            // Update User fields
            patient.User.Username = model.Username;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                // Only change password if admin entered a new one
                patient.User.PasswordHash = model.Password; // TODO: hash later
            }

            patient.User.FirstName = model.FirstName;
            patient.User.LastName = model.LastName;
            patient.User.Email = model.Email;

            // Update Patient fields
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

            if (patient == null)
                return NotFound();

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

            if (patient == null)
                return NotFound();

            // Delete the User – cascade should delete Patient if configured
            _context.Users.Remove(patient.User);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}