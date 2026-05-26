using Microsoft.AspNetCore.Mvc;
using PhysioClinicPro.Data;
using PhysioClinicPro.Models;
using PhysioClinicPro.Models.ViewModels;
using System;
using System.Linq;

namespace PhysioClinicPro.Controllers
{
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var model = new ServiceViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ServiceViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var lastService = _context.Services.OrderByDescending(s => s.Id).FirstOrDefault();
                int nextNumber = 1;
                if (lastService != null && lastService.ServiceCode != null)
                {
                    var numStr = lastService.ServiceCode.Replace("SRV", "");
                    if (int.TryParse(numStr, out int num))
                        nextNumber = num + 1;
                }

                var service = new Service
                {
                    ServiceCode = $"SRV{nextNumber:D3}",
                    ServiceName = model.ServiceName,
                    Category = model.Category ?? "Therapy",
                    Description = model.Description,
                    DefaultRate = model.DefaultRate,
                    DurationMinutes = model.DurationMinutes > 0 ? model.DurationMinutes : 30,
                    GSTPercentage = model.GSTPercentage > 0 ? model.GSTPercentage : 18,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.Services.Add(service);
                _context.SaveChanges();

                TempData["Success"] = $"Service created successfully! Code: {service.ServiceCode}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error saving service: " + ex.Message;
                return View(model);
            }
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var service = _context.Services.Find(id);
            if (service == null)
                return NotFound();

            var model = new ServiceViewModel
            {
                Id = service.Id,
                ServiceCode = service.ServiceCode,
                ServiceName = service.ServiceName,
                Category = service.Category ?? "Therapy",
                Description = service.Description,
                DefaultRate = service.DefaultRate,
                DurationMinutes = service.DurationMinutes,
                GSTPercentage = service.GSTPercentage,
                IsActive = service.IsActive,
                IsEdit = true
            };

            return View("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ServiceViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                model.IsEdit = true;
                return View("Create", model);
            }

            try
            {
                var service = _context.Services.Find(model.Id);
                if (service == null)
                    return NotFound();

                service.ServiceName = model.ServiceName;
                service.Category = model.Category;
                service.Description = model.Description;
                service.DefaultRate = model.DefaultRate;
                service.DurationMinutes = model.DurationMinutes;
                service.GSTPercentage = model.GSTPercentage;
                service.IsActive = model.IsActive;
                service.ModifiedDate = DateTime.Now;

                _context.SaveChanges();

                TempData["Success"] = "Service updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating service: " + ex.Message;
                model.IsEdit = true;
                return View("Create", model);
            }
        }

        [HttpGet]
        public IActionResult GetAll(string search = "", string category = "all")
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var query = _context.Services.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => 
                    (s.ServiceCode != null && s.ServiceCode.Contains(search)) || 
                    (s.ServiceName != null && s.ServiceName.Contains(search)));
            }

            if (category != "all")
            {
                query = query.Where(s => s.Category == category);
            }

            var services = query
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.ServiceName)
                .ToList();

            var result = services.Select(s => new
            {
                s.Id,
                ServiceCode = s.ServiceCode ?? "",
                ServiceName = s.ServiceName ?? "",
                Category = s.Category ?? "",
                Description = s.Description ?? "",
                s.DefaultRate,
                s.DurationMinutes,
                s.GSTPercentage,
                s.IsActive
            }).ToList();

            return Json(result);
        }

        public IActionResult GetServices()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var services = _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.ServiceName)
                .ToList();

            var result = services.Select(s => new
            {
                s.Id,
                ServiceCode = s.ServiceCode ?? "",
                ServiceName = s.ServiceName ?? "",
                Category = s.Category ?? "",
                s.DefaultRate,
                s.GSTPercentage,
                s.DurationMinutes
            }).ToList();

            return Json(result);
        }

        public IActionResult GetCategories()
        {
            var categories = _context.Services
                .Where(s => s.Category != null)
                .Select(s => s.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return Json(categories);
        }

        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var service = _context.Services.Find(id);
            if (service != null)
            {
                service.IsActive = false;
                _context.SaveChanges();
                TempData["Success"] = "Service deactivated successfully!";
            }

            return RedirectToAction("Index");
        }
    }
}