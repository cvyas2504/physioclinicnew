-- PhysioClinic Pro Database Script
-- Run this script in SQL Server Management Studio to create the database

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PhysioClinicPro')
BEGIN
    CREATE DATABASE PhysioClinicPro;
END
GO

USE PhysioClinicPro;
GO

-- ClinicProfiles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ClinicProfiles')
BEGIN
    CREATE TABLE ClinicProfiles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ClinicName NVARCHAR(200) NOT NULL DEFAULT 'PhysioClinic Pro',
        ClinicAddress NVARCHAR(500) NULL,
        ClinicPhone NVARCHAR(20) NULL,
        ClinicEmail NVARCHAR(100) NULL,
        LoginLogo NVARCHAR(500) NULL,
        BillingHeaderLogo NVARCHAR(500) NULL,
        BillingFooterLogo NVARCHAR(500) NULL,
        BillingHeaderText NVARCHAR(1000) NULL,
        BillingFooterText NVARCHAR(1000) NULL,
        UHIDPrefix NVARCHAR(10) NOT NULL DEFAULT 'PHY',
        InvoicePrefix NVARCHAR(10) NOT NULL DEFAULT 'INV',
        FooterMessage NVARCHAR(500) NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME NULL
    );
END
GO

-- Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(255) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'Receptionist',
        MobileNumber NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME NULL
    );
END
GO

-- Patients Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Patients')
BEGIN
    CREATE TABLE Patients (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UHID NVARCHAR(50) NOT NULL UNIQUE,
        PhotoPath NVARCHAR(500) NULL,
        PatientName NVARCHAR(200) NOT NULL,
        DateOfBirth DATETIME NULL,
        Age INT NULL,
        Gender NVARCHAR(10) NULL DEFAULT 'Male',
        MobileNumber NVARCHAR(20) NULL,
        AlternateMobile NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        Pincode NVARCHAR(10) NULL,
        EmergencyContactName NVARCHAR(200) NULL,
        EmergencyContactNumber NVARCHAR(20) NULL,
        BloodGroup NVARCHAR(10) NULL,
        ReferDoctorName NVARCHAR(200) NULL,
        MedicalHistory NVARCHAR(MAX) NULL,
        Allergies NVARCHAR(MAX) NULL,
        RegistrationDate DATETIME NOT NULL DEFAULT GETDATE(),
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

-- Services Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Services')
BEGIN
    CREATE TABLE Services (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ServiceCode NVARCHAR(20) NOT NULL UNIQUE,
        ServiceName NVARCHAR(200) NOT NULL,
        Category NVARCHAR(50) NOT NULL DEFAULT 'Therapy',
        Description NVARCHAR(500) NULL,
        DefaultRate DECIMAL(18,2) NOT NULL DEFAULT 0,
        DurationMinutes INT NOT NULL DEFAULT 30,
        GSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 18,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME NULL
    );
END
GO

-- Bills Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bills')
BEGIN
    CREATE TABLE Bills (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        BillNumber NVARCHAR(50) NOT NULL UNIQUE,
        BillDate DATETIME NOT NULL DEFAULT GETDATE(),
        PatientId INT NOT NULL FOREIGN KEY REFERENCES Patients(Id),
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        GSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        NetAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        PaidAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        DueAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Notes NVARCHAR(500) NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedBy INT NOT NULL,
        ModifiedDate DATETIME NULL,
        ModifiedBy INT NULL
    );
END
GO

-- BillDetails Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BillDetails')
BEGIN
    CREATE TABLE BillDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        BillId INT NOT NULL FOREIGN KEY REFERENCES Bills(Id),
        ServiceId INT NOT NULL FOREIGN KEY REFERENCES Services(Id),
        Quantity INT NOT NULL DEFAULT 1,
        Rate DECIMAL(18,2) NOT NULL DEFAULT 0,
        Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        GSTPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        GSTAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        NetAmount DECIMAL(18,2) NOT NULL DEFAULT 0
    );
END
GO

-- BillPayments Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BillPayments')
BEGIN
    CREATE TABLE BillPayments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        BillId INT NOT NULL FOREIGN KEY REFERENCES Bills(Id),
        Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        PaymentMode NVARCHAR(50) NOT NULL DEFAULT 'Cash',
        PaymentReference NVARCHAR(100) NULL,
        PaymentNotes NVARCHAR(500) NULL,
        PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- PatientLedgers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PatientLedgers')
BEGIN
    CREATE TABLE PatientLedgers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        PatientId INT NOT NULL FOREIGN KEY REFERENCES Patients(Id),
        TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
        TransactionType NVARCHAR(50) NOT NULL,
        ReferenceNumber NVARCHAR(50) NULL,
        Debit DECIMAL(18,2) NOT NULL DEFAULT 0,
        Credit DECIMAL(18,2) NOT NULL DEFAULT 0,
        Balance DECIMAL(18,2) NOT NULL DEFAULT 0,
        Narration NVARCHAR(500) NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- BillModifications Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BillModifications')
BEGIN
    CREATE TABLE BillModifications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        BillId INT NOT NULL FOREIGN KEY REFERENCES Bills(Id),
        ModificationType NVARCHAR(100) NOT NULL,
        OldValues NVARCHAR(500) NULL,
        NewValues NVARCHAR(500) NULL,
        Reason NVARCHAR(500) NULL,
        ModifiedBy INT NOT NULL,
        ModifiedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- BillCancellations Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BillCancellations')
BEGIN
    CREATE TABLE BillCancellations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        BillId INT NOT NULL FOREIGN KEY REFERENCES Bills(Id),
        Reason NVARCHAR(500) NOT NULL,
        CancelledBy INT NOT NULL,
        CancelledDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- BillRefunds Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BillRefunds')
BEGIN
    CREATE TABLE BillRefunds (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        BillId INT NOT NULL FOREIGN KEY REFERENCES Bills(Id),
        RefundAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        RefundMode NVARCHAR(50) NOT NULL DEFAULT 'Cash',
        Reason NVARCHAR(500) NOT NULL,
        RefundedBy INT NOT NULL,
        RefundDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- BillDiscounts Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BillDiscounts')
BEGIN
    CREATE TABLE BillDiscounts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        BillId INT NOT NULL FOREIGN KEY REFERENCES Bills(Id),
        DiscountPercentage DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Reason NVARCHAR(500) NOT NULL,
        DiscountedBy INT NOT NULL,
        DiscountDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- Insert Default Data
INSERT INTO ClinicProfiles (ClinicName, ClinicAddress, ClinicPhone, ClinicEmail, UHIDPrefix, InvoicePrefix, CreatedDate)
VALUES ('PhysioClinic Pro', '123 Healthcare Street, Medical District', '+91 9876543210', 'info@physioclinicpro.com', 'PHY', 'INV', GETDATE());

PRINT 'PhysioClinic Pro database initialized successfully!';