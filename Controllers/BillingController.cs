using Microsoft.AspNetCore.Mvc;
using PhysioClinicPro.Data;
using PhysioClinicPro.Models;
using PhysioClinicPro.Models.ViewModels;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PhysioClinicPro.Controllers
{
    public class BillingController : Controller
    {
        private readonly AppDbContext _context;

        public BillingController(AppDbContext context)
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

            var model = new BillViewModel
            {
                BillDate = DateTime.Now,
                BillDetails = new List<BillDetailViewModel>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BillViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            if (model.PatientId == 0)
            {
                TempData["Error"] = "Please select a patient";
                return View(model);
            }

            if (model.BillDetails == null || !model.BillDetails.Any())
            {
                TempData["Error"] = "Please add at least one service";
                return View(model);
            }

            try
            {
                var clinic = _context.ClinicProfiles.FirstOrDefault();
                var prefix = clinic?.InvoicePrefix ?? "INV";
                var year = DateTime.Now.Year;
                var lastBill = _context.Bills
                    .Where(b => b.BillNumber.StartsWith($"{prefix}-{year}"))
                    .OrderByDescending(b => b.BillNumber)
                    .FirstOrDefault();

                int nextNumber = 1;
                if (lastBill != null)
                {
                    var lastNumStr = lastBill.BillNumber.Split('-').LastOrDefault();
                    if (int.TryParse(lastNumStr, out int num))
                        nextNumber = num + 1;
                }

                var billNumber = $"{prefix}-{year}-{nextNumber:D5}";

                var bill = new Bill
                {
                    BillNumber = billNumber,
                    BillDate = model.BillDate,
                    PatientId = model.PatientId,
                    Notes = model.Notes,
                    Status = "Active",
                    CreatedDate = DateTime.Now,
                    CreatedBy = (int)(HttpContext.Session.GetInt32("UserId") ?? 0)
                };

                decimal totalAmount = 0;
                decimal totalGST = 0;
                decimal totalDiscount = 0;

                foreach (var detail in model.BillDetails)
                {
                    var service = _context.Services.Find(detail.ServiceId);
                    if (service == null) continue;

                    var amount = detail.Rate * detail.Quantity;
                    var discountAmt = (amount * detail.DiscountPercentage / 100) + detail.DiscountAmount;
                    var taxableAmount = amount - discountAmt;
                    var gstAmt = taxableAmount * (detail.GSTPercentage / 100);
                    var netAmt = taxableAmount + gstAmt;

                    var billDetail = new BillDetail
                    {
                        ServiceId = detail.ServiceId,
                        Quantity = detail.Quantity,
                        Rate = detail.Rate,
                        Amount = amount,
                        DiscountPercentage = detail.DiscountPercentage,
                        DiscountAmount = detail.DiscountAmount,
                        GSTPercentage = detail.GSTPercentage,
                        GSTAmount = gstAmt,
                        NetAmount = netAmt
                    };

                    bill.BillDetails.Add(billDetail);

                    totalAmount += amount;
                    totalDiscount += discountAmt;
                    totalGST += gstAmt;
                }

                bill.TotalAmount = totalAmount;
                bill.DiscountAmount = totalDiscount;
                bill.GSTAmount = totalGST;
                bill.NetAmount = totalAmount - totalDiscount + totalGST;

                var paidAmount = model.BillPayments?.Sum(p => p.Amount) ?? 0;
                bill.PaidAmount = paidAmount;
                bill.DueAmount = bill.NetAmount - paidAmount;

                _context.Bills.Add(bill);
                _context.SaveChanges();

                // Add payments
                if (model.BillPayments != null)
                {
                    foreach (var payment in model.BillPayments.Where(p => p.Amount > 0))
                    {
                        var billPayment = new BillPayment
                        {
                            BillId = bill.Id,
                            Amount = payment.Amount,
                            PaymentMode = payment.PaymentMode,
                            PaymentReference = payment.PaymentReference,
                            PaymentNotes = payment.PaymentNotes,
                            PaymentDate = DateTime.Now
                        };
                        _context.BillPayments.Add(billPayment);
                    }
                    _context.SaveChanges();
                }

                // Update patient ledger
                UpdatePatientLedger(bill.PatientId, bill.BillNumber, "Bill", bill.NetAmount, 0, $"Bill Generated - {billNumber}");

                // If partial payment, add payment entry
                if (paidAmount > 0)
                {
                    UpdatePatientLedger(bill.PatientId, bill.BillNumber, "Payment", 0, paidAmount, $"Payment received against Bill {billNumber}");
                }

                TempData["Success"] = $"Bill created successfully! Bill No: {billNumber}";
                return RedirectToAction("Print", new { id = bill.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating bill: " + ex.Message;
                return View(model);
            }
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var bill = _context.Bills
                .Include(b => b.Patient)
                .Include(b => b.BillDetails)
                    .ThenInclude(bd => bd.Service)
                .Include(b => b.BillPayments)
                .FirstOrDefault(b => b.Id == id);

            if (bill == null)
                return NotFound();

            // Only allow modification within 24 hours
            if (bill.CreatedDate.AddHours(24) < DateTime.Now)
            {
                TempData["Error"] = "Bill cannot be modified after 24 hours of creation";
                return RedirectToAction("Index");
            }

            var model = new BillViewModel
            {
                Id = bill.Id,
                BillNumber = bill.BillNumber,
                BillDate = bill.BillDate,
                PatientId = bill.PatientId,
                PatientName = bill.Patient?.PatientName,
                PatientUHID = bill.Patient?.UHID,
                TotalAmount = bill.TotalAmount,
                DiscountAmount = bill.DiscountAmount,
                GSTAmount = bill.GSTAmount,
                NetAmount = bill.NetAmount,
                PaidAmount = bill.PaidAmount,
                DueAmount = bill.DueAmount,
                Notes = bill.Notes,
                Status = bill.Status,
                IsEdit = true,
                BillDetails = bill.BillDetails.Select(bd => new BillDetailViewModel
                {
                    Id = bd.Id,
                    BillId = bd.BillId,
                    ServiceId = bd.ServiceId,
                    ServiceName = bd.Service?.ServiceName,
                    Quantity = bd.Quantity,
                    Rate = bd.Rate,
                    Amount = bd.Amount,
                    DiscountPercentage = bd.DiscountPercentage,
                    DiscountAmount = bd.DiscountAmount,
                    GSTPercentage = bd.GSTPercentage,
                    GSTAmount = bd.GSTAmount,
                    NetAmount = bd.NetAmount
                }).ToList(),
                BillPayments = bill.BillPayments.Select(bp => new BillPaymentViewModel
                {
                    Id = bp.Id,
                    BillId = bp.BillId,
                    Amount = bp.Amount,
                    PaymentMode = bp.PaymentMode,
                    PaymentReference = bp.PaymentReference,
                    PaymentNotes = bp.PaymentNotes,
                    PaymentDate = bp.PaymentDate
                }).ToList()
            };

            return View("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BillViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var bill = _context.Bills
                    .Include(b => b.BillDetails)
                    .Include(b => b.BillPayments)
                    .FirstOrDefault(b => b.Id == model.Id);

                if (bill == null)
                    return NotFound();

                // Record modification
                var modification = new BillModification
                {
                    BillId = bill.Id,
                    ModificationType = "Edit",
                    OldValues = $"Total: {bill.NetAmount}, Discount: {bill.DiscountAmount}",
                    Reason = model.Notes,
                    ModifiedBy = (int)(HttpContext.Session.GetInt32("UserId") ?? 0),
                    ModifiedDate = DateTime.Now
                };
                _context.BillModifications.Add(modification);

                // Clear old details
                _context.BillDetails.RemoveRange(bill.BillDetails);
                _context.BillPayments.RemoveRange(bill.BillPayments);

                // Recalculate
                decimal totalAmount = 0;
                decimal totalGST = 0;
                decimal totalDiscount = 0;

                foreach (var detail in model.BillDetails)
                {
                    var service = _context.Services.Find(detail.ServiceId);
                    if (service == null) continue;

                    var amount = detail.Rate * detail.Quantity;
                    var discountAmt = (amount * detail.DiscountPercentage / 100) + detail.DiscountAmount;
                    var taxableAmount = amount - discountAmt;
                    var gstAmt = taxableAmount * (detail.GSTPercentage / 100);
                    var netAmt = taxableAmount + gstAmt;

                    var billDetail = new BillDetail
                    {
                        BillId = bill.Id,
                        ServiceId = detail.ServiceId,
                        Quantity = detail.Quantity,
                        Rate = detail.Rate,
                        Amount = amount,
                        DiscountPercentage = detail.DiscountPercentage,
                        DiscountAmount = detail.DiscountAmount,
                        GSTPercentage = detail.GSTPercentage,
                        GSTAmount = gstAmt,
                        NetAmount = netAmt
                    };

                    _context.BillDetails.Add(billDetail);

                    totalAmount += amount;
                    totalDiscount += discountAmt;
                    totalGST += gstAmt;
                }

                bill.TotalAmount = totalAmount;
                bill.DiscountAmount = totalDiscount;
                bill.GSTAmount = totalGST;
                bill.NetAmount = totalAmount - totalDiscount + totalGST;

                var paidAmount = model.BillPayments?.Sum(p => p.Amount) ?? 0;
                bill.PaidAmount = paidAmount;
                bill.DueAmount = bill.NetAmount - paidAmount;
                bill.Notes = model.Notes;
                bill.ModifiedDate = DateTime.Now;
                bill.ModifiedBy = (int)(HttpContext.Session.GetInt32("UserId") ?? 0);

                if (model.BillPayments != null)
                {
                    foreach (var payment in model.BillPayments.Where(p => p.Amount > 0))
                    {
                        var billPayment = new BillPayment
                        {
                            BillId = bill.Id,
                            Amount = payment.Amount,
                            PaymentMode = payment.PaymentMode,
                            PaymentReference = payment.PaymentReference,
                            PaymentNotes = payment.PaymentNotes,
                            PaymentDate = DateTime.Now
                        };
                        _context.BillPayments.Add(billPayment);
                    }
                }

                _context.SaveChanges();

                TempData["Success"] = "Bill modified successfully!";
                return RedirectToAction("Print", new { id = bill.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error modifying bill: " + ex.Message;
                return View(model);
            }
        }

        public IActionResult Cancel(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var bill = _context.Bills.Find(id);
            if (bill == null)
                return NotFound();

            var model = new BillOperationViewModel
            {
                BillId = bill.Id,
                BillNumber = bill.BillNumber,
                CurrentAmount = bill.NetAmount
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(BillOperationViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var bill = _context.Bills.Find(model.BillId);
                if (bill == null)
                    return NotFound();

                // Record cancellation
                var cancellation = new BillCancellation
                {
                    BillId = bill.Id,
                    Reason = model.Reason ?? "",
                    CancelledBy = (int)(HttpContext.Session.GetInt32("UserId") ?? 0),
                    CancelledDate = DateTime.Now
                };
                _context.BillCancellations.Add(cancellation);

                bill.Status = "Cancelled";
                _context.SaveChanges();

                // Update patient ledger
                UpdatePatientLedger(bill.PatientId, bill.BillNumber, "Bill Cancelled", 0, bill.NetAmount, $"Bill {bill.BillNumber} Cancelled - {model.Reason}");

                TempData["Success"] = "Bill cancelled successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error cancelling bill: " + ex.Message;
                return View(model);
            }
        }

        public IActionResult Refund(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var bill = _context.Bills.Find(id);
            if (bill == null)
                return NotFound();

            var model = new BillOperationViewModel
            {
                BillId = bill.Id,
                BillNumber = bill.BillNumber,
                CurrentAmount = bill.PaidAmount,
                Amount = bill.PaidAmount
            };

            ViewBag.PaymentModes = new List<string> { "Cash", "Card", "UPI", "Net Banking", "Cheque", "RTGS", "NEFT" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Refund(BillOperationViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var bill = _context.Bills.Find(model.BillId);
                if (bill == null)
                    return NotFound();

                var refundAmount = model.Amount > bill.PaidAmount ? bill.PaidAmount : model.Amount;

                // Record refund
                var refund = new BillRefund
                {
                    BillId = bill.Id,
                    RefundAmount = refundAmount,
                    RefundMode = model.PaymentMode ?? "Cash",
                    Reason = model.Reason ?? "",
                    RefundedBy = (int)(HttpContext.Session.GetInt32("UserId") ?? 0),
                    RefundDate = DateTime.Now
                };
                _context.BillRefunds.Add(refund);

                bill.PaidAmount -= refundAmount;
                bill.DueAmount = bill.NetAmount - bill.PaidAmount;

                _context.SaveChanges();

                // Update patient ledger
                UpdatePatientLedger(bill.PatientId, bill.BillNumber, "Refund", 0, refundAmount, $"Refund of ₹{refundAmount} against Bill {bill.BillNumber} - {model.Reason}");

                TempData["Success"] = $"Refund of ₹{refundAmount} processed successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error processing refund: " + ex.Message;
                return View(model);
            }
        }

        public IActionResult Discount(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var bill = _context.Bills.Find(id);
            if (bill == null)
                return NotFound();

            var model = new BillOperationViewModel
            {
                BillId = bill.Id,
                BillNumber = bill.BillNumber,
                CurrentAmount = bill.NetAmount,
                Amount = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Discount(BillOperationViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var bill = _context.Bills.Find(model.BillId);
                if (bill == null)
                    return NotFound();

                if (model.Amount > bill.NetAmount)
                    model.Amount = bill.NetAmount;

                // Record discount
                var discount = new BillDiscount
                {
                    BillId = bill.Id,
                    DiscountPercentage = 0,
                    DiscountAmount = model.Amount,
                    Reason = model.Reason ?? "",
                    DiscountedBy = (int)(HttpContext.Session.GetInt32("UserId") ?? 0),
                    DiscountDate = DateTime.Now
                };
                _context.BillDiscounts.Add(discount);

                // Record modification
                var modification = new BillModification
                {
                    BillId = bill.Id,
                    ModificationType = "Discount",
                    OldValues = $"NetAmount: {bill.NetAmount}",
                    NewValues = $"Discount: {model.Amount}",
                    Reason = model.Reason,
                    ModifiedBy = (int)(HttpContext.Session.GetInt32("UserId") ?? 0),
                    ModifiedDate = DateTime.Now
                };
                _context.BillModifications.Add(modification);

                bill.DiscountAmount += model.Amount;
                bill.NetAmount -= model.Amount;
                bill.DueAmount = bill.NetAmount - bill.PaidAmount;

                _context.SaveChanges();

                // Update patient ledger
                UpdatePatientLedger(bill.PatientId, bill.BillNumber, "Discount", 0, model.Amount, $"Discount of ₹{model.Amount} on Bill {bill.BillNumber} - {model.Reason}");

                TempData["Success"] = $"Discount of ₹{model.Amount} applied successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error applying discount: " + ex.Message;
                return View(model);
            }
        }

        public IActionResult Print(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var bill = _context.Bills
                .Include(b => b.Patient)
                .Include(b => b.BillDetails)
                    .ThenInclude(bd => bd.Service)
                .Include(b => b.BillPayments)
                .FirstOrDefault(b => b.Id == id);

            if (bill == null)
                return NotFound();

            var clinic = _context.ClinicProfiles.FirstOrDefault();
            ViewBag.Clinic = clinic;

            return View(bill);
        }

        [HttpGet]
        public IActionResult GetAll(string search = "", string status = "all", DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var query = _context.Bills.Include(b => b.Patient).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => (b.BillNumber != null && b.BillNumber.Contains(search)) || 
                    (b.Patient != null && b.Patient.PatientName != null && b.Patient.PatientName.Contains(search)));
            }

            if (status != "all")
            {
                query = query.Where(b => b.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(b => b.BillDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(b => b.BillDate <= toDate.Value);
            }

            var bills = query
                .OrderByDescending(b => b.BillDate)
                .ThenByDescending(b => b.Id)
                .ToList();

            var result = bills.Select(b => new
            {
                b.Id,
                BillNumber = b.BillNumber ?? "",
                BillDate = b.BillDate.ToString("dd-MMM-yyyy"),
                PatientName = b.Patient?.PatientName ?? "N/A",
                PatientUHID = b.Patient?.UHID ?? "N/A",
                b.TotalAmount,
                b.DiscountAmount,
                b.GSTAmount,
                b.NetAmount,
                b.PaidAmount,
                b.DueAmount,
                Status = b.Status ?? "Active"
            }).ToList();

            return Json(result);
        }

        public IActionResult GetBill(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Json(new { error = "Unauthorized" });

            var bill = _context.Bills
                .Include(b => b.Patient)
                .Include(b => b.BillDetails)
                    .ThenInclude(bd => bd.Service)
                .Include(b => b.BillPayments)
                .FirstOrDefault(b => b.Id == id);

            if (bill == null)
                return Json(new { error = "Bill not found" });

            return Json(new
            {
                bill.Id,
                BillNumber = bill.BillNumber ?? "",
                bill.BillDate,
                PatientId = bill.PatientId,
                PatientName = bill.Patient?.PatientName ?? "",
                PatientUHID = bill.Patient?.UHID ?? "",
                bill.TotalAmount,
                bill.DiscountAmount,
                bill.GSTAmount,
                bill.NetAmount,
                bill.PaidAmount,
                bill.DueAmount,
                Status = bill.Status ?? "Active",
                Notes = bill.Notes ?? "",
                BillDetails = bill.BillDetails.Select(bd => new
                {
                    bd.Id,
                    bd.ServiceId,
                    ServiceName = bd.Service?.ServiceName ?? "",
                    bd.Quantity,
                    bd.Rate,
                    bd.Amount,
                    bd.DiscountPercentage,
                    bd.DiscountAmount,
                    bd.GSTPercentage,
                    bd.GSTAmount,
                    bd.NetAmount
                }),
                BillPayments = bill.BillPayments.Select(bp => new
                {
                    bp.Id,
                    bp.Amount,
                    PaymentMode = bp.PaymentMode ?? "Cash",
                    PaymentReference = bp.PaymentReference ?? ""
                })
            });
        }

        public IActionResult CollectPayment(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var bill = _context.Bills
                .Include(b => b.Patient)
                .FirstOrDefault(b => b.Id == id);

            if (bill == null)
                return NotFound();

            var model = new BillPaymentViewModel
            {
                BillId = bill.Id,
                Amount = bill.DueAmount
            };

            ViewBag.BillNumber = bill.BillNumber;
            ViewBag.PatientName = bill.Patient?.PatientName;
            ViewBag.DueAmount = bill.DueAmount;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CollectPayment(BillPaymentViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var bill = _context.Bills.Find(model.BillId);
                if (bill == null)
                    return NotFound();

                var payment = new BillPayment
                {
                    BillId = bill.Id,
                    Amount = model.Amount,
                    PaymentMode = model.PaymentMode,
                    PaymentReference = model.PaymentReference,
                    PaymentNotes = model.PaymentNotes,
                    PaymentDate = DateTime.Now
                };

                _context.BillPayments.Add(payment);

                bill.PaidAmount += model.Amount;
                bill.DueAmount = bill.NetAmount - bill.PaidAmount;

                _context.SaveChanges();

                // Update patient ledger
                UpdatePatientLedger(bill.PatientId, bill.BillNumber, "Payment", 0, model.Amount, $"Payment of ₹{model.Amount} received against Bill {bill.BillNumber}");

                TempData["Success"] = "Payment collected successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error collecting payment: " + ex.Message;
                return View(model);
            }
        }

        private void UpdatePatientLedger(int patientId, string reference, string type, decimal debit, decimal credit, string narration)
        {
            var lastLedger = _context.PatientLedgers
                .Where(l => l.PatientId == patientId)
                .OrderByDescending(l => l.Id)
                .FirstOrDefault();

            decimal balance = (lastLedger?.Balance ?? 0) + credit - debit;

            var ledger = new PatientLedger
            {
                PatientId = patientId,
                TransactionDate = DateTime.Now,
                TransactionType = type,
                ReferenceNumber = reference,
                Debit = debit,
                Credit = credit,
                Balance = balance,
                Narration = narration
            };

            _context.PatientLedgers.Add(ledger);
        }
    }
}