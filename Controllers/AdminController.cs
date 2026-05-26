using Microsoft.AspNetCore.Mvc;
using PhysioClinicPro.Data;
using PhysioClinicPro.Models;
using PhysioClinicPro.Models.ViewModels;
using System;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace PhysioClinicPro.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // User Master
        public IActionResult Users()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public IActionResult CreateUser()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Users");
            }

            var model = new UserViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(UserViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Users");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check if username already exists
                if (_context.Users.Any(u => u.Username == model.Username && u.Id != model.Id))
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Password = string.IsNullOrEmpty(model.Password) ? null : BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FullName = model.FullName,
                    Role = model.Role,
                    MobileNumber = model.MobileNumber,
                    Email = model.Email,
                    IsActive = model.IsActive,
                    CreatedDate = model.Id == 0 ? DateTime.Now : _context.Users.Find(model.Id)?.CreatedDate ?? DateTime.Now,
                    ModifiedDate = model.Id == 0 ? null : DateTime.Now
                };

                if (model.Id == 0)
                {
                    if (string.IsNullOrEmpty(model.Password))
                    {
                        ModelState.AddModelError("Password", "Password is required for new users");
                        return View(model);
                    }
                    _context.Users.Add(user);
                }
                else
                {
                    var existingUser = _context.Users.Find(model.Id);
                    if (existingUser == null)
                        return NotFound();

                    existingUser.Username = model.Username;
                    existingUser.FullName = model.FullName;
                    existingUser.Role = model.Role;
                    existingUser.MobileNumber = model.MobileNumber;
                    existingUser.Email = model.Email;
                    existingUser.IsActive = model.IsActive;
                    existingUser.ModifiedDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        existingUser.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    }
                }

                _context.SaveChanges();

                TempData["Success"] = model.Id == 0 ? "User created successfully!" : "User updated successfully!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error saving user: " + ex.Message;
                return View(model);
            }
        }

        public IActionResult EditUser(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Users");

            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound();

            var model = new UserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                MobileNumber = user.MobileNumber,
                Email = user.Email,
                IsActive = user.IsActive,
                IsEdit = true
            };

            return View("CreateUser", model);
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var users = _context.Users
                .OrderBy(u => u.FullName)
                .ToList();

            var result = users.Select(u => new
            {
                u.Id,
                Username = u.Username ?? "",
                FullName = u.FullName ?? "",
                Role = u.Role ?? "",
                MobileNumber = u.MobileNumber ?? "",
                Email = u.Email ?? "",
                u.IsActive,
                CreatedDate = u.CreatedDate.ToString("dd-MMM-yyyy")
            }).ToList();

            return Json(result);
        }

        public IActionResult DeleteUser(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Users");

            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.IsActive = false;
                _context.SaveChanges();
                TempData["Success"] = "User deactivated successfully!";
            }

            return RedirectToAction("Users");
        }

        // Clinic Profile
        public IActionResult ClinicProfile()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            var clinic = _context.ClinicProfiles.FirstOrDefault();
            if (clinic == null)
                return NotFound();

            var model = new ClinicProfileViewModel
            {
                Id = clinic.Id,
                ClinicName = clinic.ClinicName,
                ClinicAddress = clinic.ClinicAddress,
                ClinicPhone = clinic.ClinicPhone,
                ClinicEmail = clinic.ClinicEmail,
                LoginLogo = clinic.LoginLogo,
                BillingHeaderLogo = clinic.BillingHeaderLogo,
                BillingFooterLogo = clinic.BillingFooterLogo,
                BillingHeaderText = clinic.BillingHeaderText,
                BillingFooterText = clinic.BillingFooterText,
                UHIDPrefix = clinic.UHIDPrefix,
                InvoicePrefix = clinic.InvoicePrefix,
                FooterMessage = clinic.FooterMessage
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClinicProfile(ClinicProfileViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var clinic = _context.ClinicProfiles.Find(model.Id);
                if (clinic == null)
                    return NotFound();

                clinic.ClinicName = model.ClinicName;
                clinic.ClinicAddress = model.ClinicAddress;
                clinic.ClinicPhone = model.ClinicPhone;
                clinic.ClinicEmail = model.ClinicEmail;
                clinic.BillingHeaderText = model.BillingHeaderText;
                clinic.BillingFooterText = model.BillingFooterText;
                clinic.UHIDPrefix = model.UHIDPrefix;
                clinic.InvoicePrefix = model.InvoicePrefix;
                clinic.FooterMessage = model.FooterMessage;
                clinic.ModifiedDate = DateTime.Now;

                _context.SaveChanges();

                TempData["Success"] = "Clinic profile updated successfully!";
                return RedirectToAction("ClinicProfile");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating clinic profile: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadLogo(IFormFile file, string type)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            if (HttpContext.Session.GetString("Role") != "Admin")
                return Json(new { error = "Access denied" });

            if (file == null || file.Length == 0)
                return Json(new { error = "No file selected" });

            try
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                var fileName = $"{type}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var logoPath = $"/images/{fileName}";

                var clinic = _context.ClinicProfiles.FirstOrDefault();
                if (clinic != null)
                {
                    switch (type)
                    {
                        case "login":
                            clinic.LoginLogo = logoPath;
                            break;
                        case "header":
                            clinic.BillingHeaderLogo = logoPath;
                            break;
                        case "footer":
                            clinic.BillingFooterLogo = logoPath;
                            break;
                    }
                    _context.SaveChanges();
                }

                return Json(new { success = true, path = logoPath });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        public IActionResult GetClinicProfile()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var clinic = _context.ClinicProfiles.FirstOrDefault();
            if (clinic == null)
                return Json(new { error = "Clinic profile not found" });

            return Json(new
            {
                clinic.Id,
                clinic.ClinicName,
                clinic.ClinicAddress,
                clinic.ClinicPhone,
                clinic.ClinicEmail,
                clinic.LoginLogo,
                clinic.BillingHeaderLogo,
                clinic.BillingFooterLogo,
                clinic.BillingHeaderText,
                clinic.BillingFooterText,
                clinic.UHIDPrefix,
                clinic.InvoicePrefix,
                clinic.FooterMessage
            });
        }

        public IActionResult PatientLedger()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}