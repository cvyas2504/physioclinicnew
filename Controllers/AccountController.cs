using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PhysioClinicPro.Data;
using PhysioClinicPro.Models;
using PhysioClinicPro.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace PhysioClinicPro.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            var clinic = _context.ClinicProfiles.FirstOrDefault();
            ViewBag.ClinicName = clinic?.ClinicName ?? "PhysioClinic Pro";
            ViewBag.LoginLogo = clinic?.LoginLogo;
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            var clinic = _context.ClinicProfiles.FirstOrDefault();
            ViewBag.ClinicName = clinic?.ClinicName ?? "PhysioClinic Pro";
            ViewBag.LoginLogo = clinic?.LoginLogo;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.IsActive);
            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Role", user.Role);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}