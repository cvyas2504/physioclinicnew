using Microsoft.EntityFrameworkCore;
using PhysioClinicPro.Models;

namespace PhysioClinicPro.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ClinicProfile> ClinicProfiles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BillDetail> BillDetails { get; set; }
        public DbSet<BillPayment> BillPayments { get; set; }
        public DbSet<PatientLedger> PatientLedgers { get; set; }
        public DbSet<BillModification> BillModifications { get; set; }
        public DbSet<BillCancellation> BillCancellations { get; set; }
        public DbSet<BillRefund> BillRefunds { get; set; }
        public DbSet<BillDiscount> BillDiscounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ClinicProfile - Only one record
            modelBuilder.Entity<ClinicProfile>().HasData(new ClinicProfile
            {
                Id = 1,
                ClinicName = "PhysioClinic Pro",
                ClinicAddress = "123 Healthcare Street, Medical District",
                ClinicPhone = "+91 9876543210",
                ClinicEmail = "info@physioclinicpro.com",
                UHIDPrefix = "PHY",
                InvoicePrefix = "INV",
                CreatedDate = DateTime.Now
            });

            // Default Admin User (Password: Admin@123)
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Administrator",
                Role = "Admin",
                MobileNumber = "9876543210",
                Email = "admin@physioclinicpro.com",
                IsActive = true,
                CreatedDate = DateTime.Now
            });

            // Default Service Categories
            modelBuilder.Entity<Service>().HasData(
                new Service
                {
                    Id = 1,
                    ServiceCode = "SRV001",
                    ServiceName = "General Consultation",
                    Category = "Consultation",
                    Description = "Initial consultation with physiotherapist",
                    DefaultRate = 500,
                    DurationMinutes = 30,
                    GSTPercentage = 18,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Service
                {
                    Id = 2,
                    ServiceCode = "SRV002",
                    ServiceName = "Follow-up Consultation",
                    Category = "Consultation",
                    Description = "Follow-up visit consultation",
                    DefaultRate = 300,
                    DurationMinutes = 20,
                    GSTPercentage = 18,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Service
                {
                    Id = 3,
                    ServiceCode = "SRV003",
                    ServiceName = "Physiotherapy Session (30 min)",
                    Category = "Therapy",
                    Description = "30 minutes physiotherapy session",
                    DefaultRate = 400,
                    DurationMinutes = 30,
                    GSTPercentage = 18,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Service
                {
                    Id = 4,
                    ServiceCode = "SRV004",
                    ServiceName = "Physiotherapy Session (60 min)",
                    Category = "Therapy",
                    Description = "60 minutes physiotherapy session",
                    DefaultRate = 700,
                    DurationMinutes = 60,
                    GSTPercentage = 18,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Service
                {
                    Id = 5,
                    ServiceCode = "SRV005",
                    ServiceName = "Electrotherapy",
                    Category = "Therapy",
                    Description = "Electrotherapy treatment session",
                    DefaultRate = 350,
                    DurationMinutes = 25,
                    GSTPercentage = 18,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Service
                {
                    Id = 6,
                    ServiceCode = "SRV006",
                    ServiceName = "Ultrasound Therapy",
                    Category = "Therapy",
                    Description = "Ultrasound therapy treatment",
                    DefaultRate = 300,
                    DurationMinutes = 20,
                    GSTPercentage = 18,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Service
                {
                    Id = 7,
                    ServiceCode = "SRV007",
                    ServiceName = "Manual Therapy",
                    Category = "Therapy",
                    Description = "Manual therapy session",
                    DefaultRate = 600,
                    DurationMinutes = 45,
                    GSTPercentage = 18,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Service
                {
                    Id = 8,
                    ServiceCode = "SRV008",
                    ServiceName = "Exercise Therapy",
                    Category = "Therapy",
                    Description = "Guided exercise therapy session",
                    DefaultRate = 450,
                    DurationMinutes = 40,
                    GSTPercentage = 18,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            );
        }
    }
}