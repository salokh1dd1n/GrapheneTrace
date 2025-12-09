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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // =====================================================
        //                     ADMINS
        // =====================================================

        public async Task<IActionResult> Admins()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            return View(admins);
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Admin");
            return RedirectToAction(nameof(Admins));
        }

        // ---------- EDIT ADMIN ----------

        [HttpGet]
        public async Task<IActionResult> EditAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var vm = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = "Admin"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdmin(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.Email     = model.Email;
            user.UserName  = model.Email;
            user.FirstName = model.FirstName;
            user.LastName  = model.LastName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var e in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            return RedirectToAction(nameof(Admins));
        }

        // =====================================================
        //                    CLINICIANS
        // =====================================================

        public async Task<IActionResult> Clinicians()
        {
            var clinicians = await _context.Clinicians
                .Include(c => c.User)
                .ToListAsync();

            return View(clinicians);
        }

        [HttpGet]
        public IActionResult CreateClinician()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClinician(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Clinician");

            var clinician = new Clinician
            {
                UserId = user.Id,
                Specialisation = null,
                RegistrationNumber = null,
                AccessAllPatients = true
            };

            _context.Clinicians.Add(clinician);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Clinicians));
        }

        // ---------- EDIT CLINICIAN ----------

        [HttpGet]
        public async Task<IActionResult> EditClinician(string id)
        {
            // id = ApplicationUser Id
            var clinician = await _context.Clinicians
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == id);

            if (clinician == null) return NotFound();

            var vm = new EditUserViewModel
            {
                Id = clinician.UserId,
                Email = clinician.User.Email,
                FirstName = clinician.User.FirstName,
                LastName = clinician.User.LastName,
                Role = "Clinician",
                Specialisation = clinician.Specialisation,
                RegistrationNumber = clinician.RegistrationNumber,
                AccessAllPatients = clinician.AccessAllPatients
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClinician(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var clinician = await _context.Clinicians
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == model.Id);

            if (clinician == null) return NotFound();

            var user = clinician.User;

            user.Email     = model.Email;
            user.UserName  = model.Email;
            user.FirstName = model.FirstName;
            user.LastName  = model.LastName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var e in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            clinician.Specialisation     = model.Specialisation;
            clinician.RegistrationNumber = model.RegistrationNumber;
            if (model.AccessAllPatients.HasValue)
                clinician.AccessAllPatients = model.AccessAllPatients.Value;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Clinicians));
        }

        // =====================================================
        //                      PATIENTS
        // =====================================================

        public async Task<IActionResult> Patients()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .ToListAsync();

            return View(patients);
        }

        [HttpGet]
        public IActionResult CreatePatient()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePatient(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Patient");

            var patient = new Patient
            {
                UserId = user.Id,
                DateOfBirth = model.DateOfBirth
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Patients));
        }

        // ---------- EDIT PATIENT ----------

        [HttpGet]
        public async Task<IActionResult> EditPatient(string id)
        {
            // id = ApplicationUser Id
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (patient == null) return NotFound();

            var vm = new EditUserViewModel
            {
                Id = patient.UserId,
                Email = patient.User.Email,
                FirstName = patient.User.FirstName,
                LastName = patient.User.LastName,
                Role = "Patient",
                DateOfBirth = patient.DateOfBirth
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == model.Id);

            if (patient == null) return NotFound();

            var user = patient.User;

            user.Email     = model.Email;
            user.UserName  = model.Email;
            user.FirstName = model.FirstName;
            user.LastName  = model.LastName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var e in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            patient.DateOfBirth = model.DateOfBirth;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Patients));
        }
        
        // =====================================================
        //                 ADMIN RESET PASSWORD
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var vm = new AdminResetPasswordViewModel
            {
                UserId = user.Id,
                Email = user.Email
            };

            return View(vm); // Views/Admin/ResetPassword.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AdminResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                return View(model);
            }

            // redirect back to correct list depending on role
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Patient"))
                return RedirectToAction(nameof(Patients));

            if (roles.Contains("Clinician"))
                return RedirectToAction(nameof(Clinicians));

            // default / Admin
            return RedirectToAction(nameof(Admins));
        }
    }
}