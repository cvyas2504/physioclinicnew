using Microsoft.AspNetCore.Mvc;
using PhysioClinicPro.Data;
using PhysioClinicPro.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PhysioClinicPro.Controllers
{
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            var clinic = _context.ClinicProfiles.FirstOrDefault();
            ViewBag.ClinicName = clinic?.ClinicName ?? "PhysioClinic Pro";

            return View();
        }

        [HttpGet]
        public IActionResult PatientReport(DateTime? fromDate, DateTime? toDate, string status = "all")
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var query = _context.Patients.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.RegistrationDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.RegistrationDate <= toDate.Value);
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
                    p.Address,
                    p.City,
                    p.State,
                    RegistrationDate = p.RegistrationDate.ToString("dd-MMM-yyyy"),
                    p.IsActive,
                    RegistrationType = p.RegistrationDate.Date == DateTime.Today ? "New" : "Revisit"
                })
                .ToList();

            return Json(patients);
        }

        [HttpGet]
        public IActionResult BillingReport(DateTime? fromDate, DateTime? toDate, string status = "all", string paymentMode = "all")
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var query = _context.Bills
                .Include(b => b.Patient)
                .Include(b => b.BillPayments)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(b => b.BillDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(b => b.BillDate <= toDate.Value);
            }

            if (status != "all")
            {
                query = query.Where(b => b.Status == status);
            }

            var bills = query
                .OrderByDescending(b => b.BillDate)
                .ToList();

            if (paymentMode != "all")
            {
                bills = bills.Where(b => b.BillPayments.Any(p => p.PaymentMode == paymentMode)).ToList();
            }

            var result = bills.Select(b => new
            {
                b.Id,
                b.BillNumber,
                BillDate = b.BillDate.ToString("dd-MMM-yyyy"),
                PatientName = b.Patient != null ? b.Patient.PatientName : "N/A",
                PatientUHID = b.Patient != null ? b.Patient.UHID : "N/A",
                b.TotalAmount,
                b.DiscountAmount,
                b.GSTAmount,
                b.NetAmount,
                b.PaidAmount,
                b.DueAmount,
                PaymentModes = string.Join(", ", b.BillPayments.Select(p => p.PaymentMode).Distinct()),
                b.Status
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public IActionResult PatientLedgerReport(int? patientId, DateTime? fromDate, DateTime? toDate)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var query = _context.PatientLedgers
                .Include(l => l.Patient)
                .AsQueryable();

            if (patientId.HasValue)
            {
                query = query.Where(l => l.PatientId == patientId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.TransactionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.TransactionDate <= toDate.Value);
            }

            var ledger = query
                .OrderBy(l => l.PatientId)
                .ThenByDescending(l => l.TransactionDate)
                .ThenByDescending(l => l.Id)
                .Select(l => new
                {
                    l.Id,
                    PatientId = l.PatientId,
                    PatientName = l.Patient != null ? l.Patient.PatientName : "N/A",
                    PatientUHID = l.Patient != null ? l.Patient.UHID : "N/A",
                    TransactionDate = l.TransactionDate.ToString("dd-MMM-yyyy HH:mm"),
                    l.TransactionType,
                    l.ReferenceNumber,
                    l.Debit,
                    l.Credit,
                    l.Balance,
                    l.Narration
                })
                .ToList();

            return Json(ledger);
        }

        [HttpGet]
        public IActionResult RevenueReport(DateTime? fromDate, DateTime? toDate, string period = "daily")
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var startDate = fromDate ?? DateTime.Today.AddMonths(-1);
            var endDate = toDate ?? DateTime.Today;

            var payments = _context.BillPayments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .ToList();

            var bills = _context.Bills
                .Where(b => b.BillDate >= startDate && b.BillDate <= endDate)
                .Include(b => b.BillDetails)
                    .ThenInclude(bd => bd.Service)
                .ToList();

            // Summary
            var summary = new
            {
                TotalCollection = payments.Sum(p => p.Amount),
                TotalBilling = bills.Where(b => b.Status == "Active").Sum(b => b.NetAmount),
                TotalDue = bills.Where(b => b.Status == "Active").Sum(b => b.DueAmount),
                CancelledBills = bills.Count(b => b.Status == "Cancelled"),
                TotalDiscount = bills.Where(b => b.Status == "Active").Sum(b => b.DiscountAmount)
            };

            // By Payment Mode
            var byPaymentMode = payments
                .GroupBy(p => p.PaymentMode)
                .Select(g => new
                {
                    PaymentMode = g.Key,
                    TotalAmount = g.Sum(p => p.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            // By Service Category
            var byCategory = bills
                .Where(b => b.Status == "Active")
                .SelectMany(b => b.BillDetails)
                .GroupBy(bd => bd.Service?.Category ?? "Unknown")
                .Select(g => new
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(bd => bd.NetAmount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            // Daily Summary
            var dailySummary = payments
                .GroupBy(p => p.PaymentDate.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("dd-MMM-yyyy"),
                    TotalCollection = g.Sum(p => p.Amount),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            return Json(new
            {
                Summary = summary,
                ByPaymentMode = byPaymentMode,
                ByCategory = byCategory,
                DailySummary = dailySummary
            });
        }

        [HttpGet]
        public IActionResult GetPatientsForLedger()
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
                    Display = $"{p.UHID} - {p.PatientName}"
                })
                .ToList();

            return Json(patients);
        }
    }
}