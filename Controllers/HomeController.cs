using Microsoft.AspNetCore.Mvc;
using PhysioClinicPro.Data;
using System.Linq;
using System;

namespace PhysioClinicPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            // Dashboard Statistics
            ViewBag.TotalPatients = _context.Patients.Count();
            ViewBag.TodayPatients = _context.Patients.Count(p => p.RegistrationDate.Date == DateTime.Today);
            ViewBag.TotalBills = _context.Bills.Count();
            ViewBag.TodayRevenue = _context.BillPayments.Where(p => p.PaymentDate.Date == DateTime.Today).Sum(p => p.Amount);
            ViewBag.TotalDue = _context.Bills.Where(b => b.Status == "Active").Sum(b => b.DueAmount);
            ViewBag.Services = _context.Services.Count(s => s.IsActive);

            var clinic = _context.ClinicProfiles.FirstOrDefault();
            ViewBag.ClinicName = clinic?.ClinicName ?? "PhysioClinic Pro";

            return View();
        }
    }
}