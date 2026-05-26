using System.ComponentModel.DataAnnotations;

namespace PhysioClinicPro.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }

    public class UserViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = "";

        [MaxLength(255)]
        public string? Password { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Receptionist";

        [MaxLength(20)]
        public string MobileNumber { get; set; } = "";

        [MaxLength(100)]
        public string Email { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public bool IsEdit { get; set; }
    }

    public class PatientViewModel
    {
        public int Id { get; set; }
        public string? UHID { get; set; }
        public string? PhotoPath { get; set; }

        [Required]
        [MaxLength(200)]
        public string? PatientName { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(20)]
        public string? MobileNumber { get; set; }

        [MaxLength(20)]
        public string? AlternateMobile { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(10)]
        public string? Pincode { get; set; }

        [MaxLength(200)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactNumber { get; set; }

        [MaxLength(10)]
        public string? BloodGroup { get; set; }

        [MaxLength(200)]
        public string? ReferDoctorName { get; set; }

        public string? MedicalHistory { get; set; }
        public string? Allergies { get; set; }

        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsEdit { get; set; }
    }

    public class ServiceViewModel
    {
        public int Id { get; set; }
        public string? ServiceCode { get; set; }

        [Required]
        [MaxLength(200)]
        public string? ServiceName { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, 100000)]
        public decimal DefaultRate { get; set; } = 0;

        public int DurationMinutes { get; set; } = 30;

        [Range(0, 100)]
        public decimal GSTPercentage { get; set; } = 18;

        public bool IsActive { get; set; } = true;
        public bool IsEdit { get; set; }
    }

    public class BillViewModel
    {
        public int Id { get; set; }
        public string? BillNumber { get; set; }
        public DateTime BillDate { get; set; } = DateTime.Now;
        public int PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientUHID { get; set; }
        public decimal TotalAmount { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal GSTAmount { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;
        public decimal PaidAmount { get; set; } = 0;
        public decimal DueAmount { get; set; } = 0;
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public bool IsEdit { get; set; }

        public List<BillDetailViewModel> BillDetails { get; set; } = new();
        public List<BillPaymentViewModel> BillPayments { get; set; } = new();
    }

    public class BillDetailViewModel
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Rate { get; set; } = 0;
        public decimal Amount { get; set; } = 0;
        public decimal DiscountPercentage { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal GSTPercentage { get; set; } = 0;
        public decimal GSTAmount { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;
    }

    public class BillPaymentViewModel
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public decimal Amount { get; set; } = 0;
        public string? PaymentMode { get; set; }
        public string? PaymentReference { get; set; }
        public string? PaymentNotes { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }

    public class BillOperationViewModel
    {
        public int BillId { get; set; }
        public string? BillNumber { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public string? PaymentMode { get; set; }
    }

    public class PatientLedgerViewModel
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? PatientUHID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? TransactionType { get; set; }
        public string? ReferenceNumber { get; set; }
        public decimal Debit { get; set; } = 0;
        public decimal Credit { get; set; } = 0;
        public decimal Balance { get; set; } = 0;
        public string? Narration { get; set; }
    }

    public class ReportFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? PatientId { get; set; }
        public string? Status { get; set; }
        public string? PaymentMode { get; set; }
        public string? ServiceCategory { get; set; }
    }

    public class ClinicProfileViewModel
    {
        public int Id { get; set; }
        public string ClinicName { get; set; } = "";
        public string ClinicAddress { get; set; } = "";
        public string ClinicPhone { get; set; } = "";
        public string ClinicEmail { get; set; } = "";
        public string? LoginLogo { get; set; }
        public string? BillingHeaderLogo { get; set; }
        public string? BillingFooterLogo { get; set; }
        public string BillingHeaderText { get; set; } = "";
        public string BillingFooterText { get; set; } = "";
        public string UHIDPrefix { get; set; } = "PHY";
        public string InvoicePrefix { get; set; } = "INV";
        public string FooterMessage { get; set; } = "";
    }
}