// using System.Security.Claims;
// using GrapheneTrace.Data;
// using GrapheneTrace.Models;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using Microsoft.AspNetCore.Mvc;
//
// namespace GrapheneTrace.Controllers;
//
// public class AccountController : Controller
// {
//     private readonly AppDbContext _context;
//
//     public AccountController(AppDbContext context)
//     {
//         _context = context;
//     }
//
//     [HttpGet]
//     public IActionResult Login() => View();
//
//     [HttpPost]
//     public async Task<IActionResult> Login(LoginViewModel model)
//     {
//         var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);
//
//         if (user == null || user.PasswordHash != model.Password)
//         {
//             ModelState.AddModelError("", "Invalid username or password");
//             return View(model);
//         }
//
//         // Create claims
//         var claims = new List<Claim>
//         {
//             new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
//             new Claim(ClaimTypes.Name, user.Username),
//             new Claim(ClaimTypes.Role, user.Role.ToString())
//         };
//
//         var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//         var principal = new ClaimsPrincipal(identity);
//
//         await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
//
//         // Redirect based on Role
//         return user.Role switch
//         {
//             UserRole.Admin => RedirectToAction("Index", "Admin"),
//             UserRole.Clinician => RedirectToAction("Index", "Clinician"),
//             UserRole.Patient => RedirectToAction("Index", "Patient"),
//             _ => RedirectToAction("Index", "Home"),
//         };
//     }
//
//     public async Task<IActionResult> Logout()
//     {
//         await HttpContext.SignOutAsync();
//         return RedirectToAction("Login");
//     }
// }