using Microsoft.EntityFrameworkCore;
using PhysioClinicPro.Data;
using System.Text.Json.Serialization;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Database connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".PhysioClinic.Session";
});

// Add HttpContextAccessor for session access
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        // Create database and apply migrations
        context.Database.EnsureCreated();
        
        // Seed data if tables are empty
        if (!context.ClinicProfiles.Any())
        {
            var clinic = new PhysioClinicPro.Models.ClinicProfile
            {
                ClinicName = "PhysioClinic Pro",
                ClinicAddress = "123 Healthcare Street, Medical District",
                ClinicPhone = "+91 9876543210",
                ClinicEmail = "info@physioclinicpro.com",
                UHIDPrefix = "PHY",
                InvoicePrefix = "INV",
                CreatedDate = DateTime.Now
            };
            context.ClinicProfiles.Add(clinic);
            context.SaveChanges();
        }
        
        if (!context.Users.Any())
        {
            var admin = new PhysioClinicPro.Models.User
            {
                Username = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Administrator",
                Role = "Admin",
                MobileNumber = "9876543210",
                Email = "admin@physioclinicpro.com",
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            context.Users.Add(admin);
            context.SaveChanges();
        }
        
        if (!context.Services.Any())
        {
            var servicesList = new List<PhysioClinicPro.Models.Service>
            {
                new() { ServiceCode = "SRV001", ServiceName = "General Consultation", Category = "Consultation", Description = "Initial consultation with physiotherapist", DefaultRate = 500, DurationMinutes = 30, GSTPercentage = 18, IsActive = true, CreatedDate = DateTime.Now },
                new() { ServiceCode = "SRV002", ServiceName = "Follow-up Consultation", Category = "Consultation", Description = "Follow-up visit consultation", DefaultRate = 300, DurationMinutes = 20, GSTPercentage = 18, IsActive = true, CreatedDate = DateTime.Now },
                new() { ServiceCode = "SRV003", ServiceName = "Physiotherapy Session (30 min)", Category = "Therapy", Description = "30 minutes physiotherapy session", DefaultRate = 400, DurationMinutes = 30, GSTPercentage = 18, IsActive = true, CreatedDate = DateTime.Now },
                new() { ServiceCode = "SRV004", ServiceName = "Physiotherapy Session (60 min)", Category = "Therapy", Description = "60 minutes physiotherapy session", DefaultRate = 700, DurationMinutes = 60, GSTPercentage = 18, IsActive = true, CreatedDate = DateTime.Now },
                new() { ServiceCode = "SRV005", ServiceName = "Electrotherapy", Category = "Therapy", Description = "Electrotherapy treatment session", DefaultRate = 350, DurationMinutes = 25, GSTPercentage = 18, IsActive = true, CreatedDate = DateTime.Now },
                new() { ServiceCode = "SRV006", ServiceName = "Ultrasound Therapy", Category = "Therapy", Description = "Ultrasound therapy treatment", DefaultRate = 300, DurationMinutes = 20, GSTPercentage = 18, IsActive = true, CreatedDate = DateTime.Now },
                new() { ServiceCode = "SRV007", ServiceName = "Manual Therapy", Category = "Therapy", Description = "Manual therapy session", DefaultRate = 600, DurationMinutes = 45, GSTPercentage = 18, IsActive = true, CreatedDate = DateTime.Now },
                new() { ServiceCode = "SRV008", ServiceName = "Exercise Therapy", Category = "Therapy", Description = "Guided exercise therapy session", DefaultRate = 450, DurationMinutes = 40, GSTPercentage = 18, IsActive = true, CreatedDate = DateTime.Now }
            };
            context.Services.AddRange(servicesList);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

app.Run();