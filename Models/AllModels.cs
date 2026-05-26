using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhysioClinicPro.Models
{
    public class ClinicProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ClinicName { get; set; } = "PhysioClinic Pro";

        [MaxLength(500)]
        public string ClinicAddress { get; set; } = "";

        [MaxLength(20)]
        public string ClinicPhone { get; set; } = "";

        [MaxLength(100)]
        public string ClinicEmail { get; set; } = "";

        [MaxLength(500)]
        public string? LoginLogo { get; set; }

        [MaxLength(500)]
        public string? BillingHeaderLogo { get; set; }

        [MaxLength(500)]
        public string? BillingFooterLogo { get; set; }

        [MaxLength(1000)]
        public string BillingHeaderText { get; set; } = "";

        [MaxLength(1000)]
        public string BillingFooterText { get; set; } = "";

        [MaxLength(10)]
        public string UHIDPrefix { get; set; } = "PHY";

        [MaxLength(10)]
        public string InvoicePrefix { get; set; } = "INV";

        [MaxLength(500)]
        public string FooterMessage { get; set; } = "";

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Password { get; set; }

        [Required]
        [MaxLength(100)]
        public string? FullName { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Role { get; set; }

        [MaxLength(20)]
        public string? MobileNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
    }

    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string? UHID { get; set; }

        [MaxLength(200)]
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
        public string Address { get; set; } = "";

        [MaxLength(100)]
        public string City { get; set; } = "";

        [MaxLength(100)]
        public string State { get; set; } = "";

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

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        [NotMapped]
        public string? SearchUHID => $"UHID: {UHID}";

        [NotMapped]
        public string? DisplayName => $"{UHID} - {PatientName}";
    }

    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string? ServiceCode { get; set; }

        [Required]
        [MaxLength(200)]
        public string? ServiceName { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultRate { get; set; } = 0;

        public int DurationMinutes { get; set; } = 30;

        [Column(TypeName = "decimal(5,2)")]
        public decimal GSTPercentage { get; set; } = 18;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
    }

    public class Bill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string? BillNumber { get; set; }

        public DateTime BillDate { get; set; } = DateTime.Now;

        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GSTAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DueAmount { get; set; } = 0;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

        public ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
        public ICollection<BillPayment> BillPayments { get; set; } = new List<BillPayment>();
    }

    public class BillDetail
    {
        [Key]
        public int Id { get; set; }

        public int BillId { get; set; }

        [ForeignKey("BillId")]
        public Bill? Bill { get; set; }

        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public Service? Service { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GSTPercentage { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GSTAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; } = 0;
    }

    public class BillPayment
    {
        [Key]
        public int Id { get; set; }

        public int BillId { get; set; }

        [ForeignKey("BillId")]
        public Bill? Bill { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0;

        [Required]
        [MaxLength(50)]
        public string? PaymentMode { get; set; }

        [MaxLength(100)]
        public string? PaymentReference { get; set; }

        [MaxLength(500)]
        public string? PaymentNotes { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class PatientLedger
    {
        [Key]
        public int Id { get; set; }

        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string? TransactionType { get; set; }

        [MaxLength(50)]
        public string? ReferenceNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        [MaxLength(500)]
        public string? Narration { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class BillModification
    {
        [Key]
        public int Id { get; set; }

        public int BillId { get; set; }

        [MaxLength(100)]
        public string? ModificationType { get; set; }

        [MaxLength(500)]
        public string? OldValues { get; set; }

        [MaxLength(500)]
        public string? NewValues { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public int ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; } = DateTime.Now;
    }

    public class BillCancellation
    {
        [Key]
        public int Id { get; set; }

        public int BillId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public int CancelledBy { get; set; }

        public DateTime CancelledDate { get; set; } = DateTime.Now;
    }

    public class BillRefund
    {
        [Key]
        public int Id { get; set; }

        public int BillId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; } = 0;

        [MaxLength(50)]
        public string? RefundMode { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public int RefundedBy { get; set; }

        public DateTime RefundDate { get; set; } = DateTime.Now;
    }

    public class BillDiscount
    {
        [Key]
        public int Id { get; set; }

        public int BillId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [MaxLength(500)]
        public string? Reason { get; set; }

        public int DiscountedBy { get; set; }

        public DateTime DiscountDate { get; set; } = DateTime.Now;
    }
}