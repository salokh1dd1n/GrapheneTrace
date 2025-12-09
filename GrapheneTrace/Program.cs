using Microsoft.EntityFrameworkCore;
using GrapheneTrace.Data;
using GrapheneTrace.Models;
using Microsoft.AspNetCore.Identity;
using GrapheneTrace.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// 1) Register services
// ---------------------------------------------------------

// DbContext & SQL Server connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Services
builder.Services.AddSingleton(new PressureAnalysisService());
builder.Services.AddScoped<PressureAnalysisService>();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
//     {
//         // Strong password rules
//         options.Password.RequiredLength = 8;
//         options.Password.RequireDigit = true;
//         options.Password.RequireLowercase = true;
//         options.Password.RequireUppercase = true;
//         options.Password.RequireNonAlphanumeric = true;
//
//         // (Optional) lockout / user rules can stay default for now
//     })
//     .AddEntityFrameworkStores<AppDbContext>()
//     .AddDefaultTokenProviders();

// Cookie paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// MVC
builder.Services.AddControllersWithViews();

// ---------------------------------------------------------
// 2) Build app
// ---------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------
// 3) Seed roles + default admins (AFTER build, BEFORE pipeline)
// ---------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeed.SeedAsync(services);
}

// ---------------------------------------------------------
// 4) Middleware pipeline
// ---------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// In .NET 9 templates MapStaticAssets replaces UseStaticFiles,
// you already use MapStaticAssets below so no UseStaticFiles needed.

app.UseRouting();

app.UseAuthentication();   // ⭐ IMPORTANT: must be before UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();