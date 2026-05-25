using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PhysioClinicPro.Data;
using PhysioClinicPro.Models;
using PhysioClinicPro.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PhysioClinicPro.Controllers
{
    public class PatientController : Controller
    {
        private readonly AppDbContext _context;

        public PatientController(AppDbContext context)
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

            var model = new PatientViewModel
            {
                RegistrationDate = DateTime.Now
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PatientViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var clinic = _context.ClinicProfiles.FirstOrDefault();
                var prefix = clinic?.UHIDPrefix ?? "PHY";
                var year = DateTime.Now.Year;
                var lastPatient = _context.Patients
                    .Where(p => p.UHID.StartsWith($"{prefix}-{year}"))
                    .OrderByDescending(p => p.UHID)
                    .FirstOrDefault();

                int nextNumber = 1;
                if (lastPatient != null)
                {
                    var lastNumber = lastPatient.UHID.Split('-').LastOrDefault();
                    if (int.TryParse(lastNumber, out int num))
                        nextNumber = num + 1;
                }

                var uhid = $"{prefix}-{year}-{nextNumber:D5}";

                var patient = new Patient
                {
                    UHID = uhid,
                    PatientName = model.PatientName,
                    DateOfBirth = model.DateOfBirth,
                    Age = model.Age,
                    Gender = model.Gender,
                    MobileNumber = model.MobileNumber,
                    AlternateMobile = model.AlternateMobile,
                    Email = model.Email,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    Pincode = model.Pincode,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactNumber = model.EmergencyContactNumber,
                    BloodGroup = model.BloodGroup,
                    ReferDoctorName = model.ReferDoctorName,
                    MedicalHistory = model.MedicalHistory,
                    Allergies = model.Allergies,
                    RegistrationDate = model.RegistrationDate,
                    IsActive = true
                };

                _context.Patients.Add(patient);
                _context.SaveChanges();

                TempData["Success"] = $"Patient registered successfully! UHID: {uhid}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error saving patient: " + ex.Message;
                return View(model);
            }
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var patient = _context.Patients.Find(id);
            if (patient == null)
                return NotFound();

            var model = new PatientViewModel
            {
                Id = patient.Id,
                UHID = patient.UHID,
                PhotoPath = patient.PhotoPath,
                PatientName = patient.PatientName,
                DateOfBirth = patient.DateOfBirth,
                Age = patient.Age,
                Gender = patient.Gender,
                MobileNumber = patient.MobileNumber,
                AlternateMobile = patient.AlternateMobile,
                Email = patient.Email,
                Address = patient.Address,
                City = patient.City,
                State = patient.State,
                Pincode = patient.Pincode,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactNumber = patient.EmergencyContactNumber,
                BloodGroup = patient.BloodGroup,
                ReferDoctorName = patient.ReferDoctorName,
                MedicalHistory = patient.MedicalHistory,
                Allergies = patient.Allergies,
                RegistrationDate = patient.RegistrationDate,
                IsActive = patient.IsActive,
                IsEdit = true
            };

            return View("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(PatientViewModel model)
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
                var patient = _context.Patients.Find(model.Id);
                if (patient == null)
                    return NotFound();

                patient.PatientName = model.PatientName;
                patient.DateOfBirth = model.DateOfBirth;
                patient.Age = model.Age;
                patient.Gender = model.Gender;
                patient.MobileNumber = model.MobileNumber;
                patient.AlternateMobile = model.AlternateMobile;
                patient.Email = model.Email;
                patient.Address = model.Address;
                patient.City = model.City;
                patient.State = model.State;
                patient.Pincode = model.Pincode;
                patient.EmergencyContactName = model.EmergencyContactName;
                patient.EmergencyContactNumber = model.EmergencyContactNumber;
                patient.BloodGroup = model.BloodGroup;
                patient.ReferDoctorName = model.ReferDoctorName;
                patient.MedicalHistory = model.MedicalHistory;
                patient.Allergies = model.Allergies;
                patient.IsActive = model.IsActive;

                _context.SaveChanges();

                TempData["Success"] = "Patient updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating patient: " + ex.Message;
                model.IsEdit = true;
                return View("Create", model);
            }
        }

        public IActionResult GetPatients()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var patients = _context.Patients
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.UHID,
                    p.PatientName,
                    p.MobileNumber,
                    Display = $"{p.UHID} - {p.PatientName} ({p.MobileNumber})"
                })
                .ToList();

            return Json(patients);
        }

        public IActionResult SearchPatients(string term)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var patients = _context.Patients
                .Where(p => p.IsActive && 
                    (p.UHID.Contains(term) || 
                     p.PatientName.Contains(term) || 
                     p.MobileNumber.Contains(term)))
                .Select(p => new
                {
                    p.Id,
                    p.UHID,
                    p.PatientName,
                    p.MobileNumber,
                    Display = $"{p.UHID} - {p.PatientName}"
                })
                .Take(20)
                .ToList();

            return Json(patients);
        }

        public IActionResult GetPatient(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var patient = _context.Patients.Find(id);
            if (patient == null)
                return Json(new { error = "Patient not found" });

            return Json(new
            {
                patient.Id,
                patient.UHID,
                patient.PatientName,
                patient.Gender,
                patient.Age,
                patient.MobileNumber,
                patient.Email,
                patient.Address
            });
        }

        [HttpGet]
        public IActionResult GetAll(string search = "", string status = "all")
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var query = _context.Patients.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => 
                    p.UHID.Contains(search) || 
                    p.PatientName.Contains(search) || 
                    p.MobileNumber.Contains(search));
            }

            if (status != "all")
            {
                query = query.Where(p => p.IsActive == (status == "active"));
            }

            var patients = query
                .OrderByDescending(p => p.RegistrationDate)
                .Select(p => new
                {
                    p.Id,
                    p.UHID,
                    p.PatientName,
                    p.Gender,
                    p.Age,
                    p.MobileNumber,
                    p.Email,
                    RegistrationDate = p.RegistrationDate.ToString("dd-MMM-yyyy"),
                    p.IsActive
                })
                .ToList();

            return Json(patients);
        }

        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var patient = _context.Patients.Find(id);
            if (patient != null)
            {
                patient.IsActive = false;
                _context.SaveChanges();
                TempData["Success"] = "Patient deactivated successfully!";
            }

            return RedirectToAction("Index");
        }
    }
}