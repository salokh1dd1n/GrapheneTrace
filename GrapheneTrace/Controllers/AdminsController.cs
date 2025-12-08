using System.Threading.Tasks;
using GrapheneTrace.Data;
using GrapheneTrace.Models;
using GrapheneTrace.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Controllers
{
    public class AdminsController : Controller
    {
        private readonly AppDbContext _context;

        public AdminsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admins
        public async Task<IActionResult> Index()
        {
            var admins = await _context.Admins
                .Include(a => a.User)
                .ToListAsync();

            return View(admins);
        }

        // GET: Admins/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var admin = await _context.Admins
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (admin == null)
                return NotFound();

            return View(admin);
        }

        // GET: Admins/Create
        public IActionResult Create()
        {
            var vm = new AdminFormViewModel();
            return View(vm);
        }

        // POST: Admins/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                Username = model.Username,
                PasswordHash = model.Password, // TODO: hash later
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Role = UserRole.Admin
            };

            var admin = new Admin
            {
                User = user,
                Notes = model.Notes
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var admin = await _context.Admins
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (admin == null)
                return NotFound();

            var vm = new AdminFormViewModel
            {
                Id = admin.Id,
                Username = admin.User.Username,
                FirstName = admin.User.FirstName,
                LastName = admin.User.LastName,
                Email = admin.User.Email,
                Notes = admin.Notes
                // Password left empty intentionally
            };

            return View(vm);
        }

        // POST: Admins/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminFormViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var admin = await _context.Admins
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (admin == null)
                return NotFound();

            // Update User
            admin.User.Username = model.Username;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                admin.User.PasswordHash = model.Password; // TODO: hash later
            }

            admin.User.FirstName = model.FirstName;
            admin.User.LastName = model.LastName;
            admin.User.Email = model.Email;

            // Update admin fields
            admin.Notes = model.Notes;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admins/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var admin = await _context.Admins
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (admin == null)
                return NotFound();

            return View(admin);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var admin = await _context.Admins
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (admin == null)
                return NotFound();

            _context.Users.Remove(admin.User); // cascades to Admin
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}