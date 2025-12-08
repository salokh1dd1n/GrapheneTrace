using Microsoft.EntityFrameworkCore;
using GrapheneTrace.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Add DbContext & SQL Server connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Cookie Authentication
// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(options =>
//     {
//         options.LoginPath = "/Account/Login";
//         options.AccessDeniedPath = "/Account/AccessDenied";
//     });
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//
//     // Apply any pending migrations (safe even if already applied)
//     db.Database.Migrate();
//
//     // Only seed if there are no users yet
//     if (!db.Users.Any())
//     {
//         db.Users.AddRange(
//             new User
//             {
//                 Username = "admin",
//                 PasswordHash = "admin123",  // plain text for now (OK for uni project)
//                 FullName = "Admin User",
//                 Email = "admin@example.com",
//                 Role = UserRole.Admin
//             },
//             new User
//             {
//                 Username = "clinician",
//                 PasswordHash = "clinician123",
//                 FullName = "Clinician User",
//                 Email = "clinician@example.com",
//                 Role = UserRole.Clinician
//             },
//             new User
//             {
//                 Username = "patient",
//                 PasswordHash = "patient123",
//                 FullName = "Patient User",
//                 Email = "patient@example.com",
//                 Role = UserRole.Patient
//             }
//         );
//
//         db.SaveChanges();
//     }
// }


app.Run();