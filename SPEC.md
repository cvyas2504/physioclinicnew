# PhysioClinic Pro - Physiotherapy Clinic Management System

## Project Overview

**Project Name:** PhysioClinic Pro
**Project Type:** Web Application (.NET 8 MVC with SQL Server)
**Core Functionality:** Comprehensive clinic management system for physiotherapy clinics covering patient management, billing, reporting, and administrative functions.
**Target Users:** Clinic administrators, receptionists, physiotherapists, billing staff, and clinic owners.

---

## Technology Stack

- **Framework:** .NET 8 MVC
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Identity
- **Frontend:** Bootstrap 5, DataTables, Font Awesome
- **Reports:** RDLC Reports
- **Architecture:** Clean Architecture with Repository Pattern

---

## Module Specifications

### 1. Authentication & Settings

#### Login Page
- Clinic logo display
- Clinic name display
- Username/password fields
- Remember me option
- Forgot password link

#### User Master
- User ID (auto-generated)
- Username (unique)
- Password (encrypted)
- Full Name
- Role (Admin, Receptionist, Physiotherapist, Billing Staff)
- Mobile Number
- Email
- Status (Active/Inactive)
- Created Date
- Modified Date

### 2. Clinic Profile Master
- Clinic Name
- Clinic Address
- Clinic Phone
- Clinic Email
- Logo (for login page)
- Billing Header Logo
- Billing Footer Logo
- Billing Header Text
- Billing Footer Text
- UHID Prefix (configurable, default: "PHY")
- Invoice Prefix
- Footer Message

### 3. Patient Registration
- UHID (auto-generated with prefix, e.g., PHY-2024-00001)
- Patient Photo
- Patient Name (Mandatory)
- Date of Birth
- Age
- Gender (Male/Female/Other)
- Mobile Number
- Alternate Mobile
- Email
- Address
- City
- State
- Pincode
- Emergency Contact Name
- Emergency Contact Number
- Blood Group
- Refer Doctor Name
- Medical History (textarea)
- Allergies (textarea)
- Registration Date
- Status (Active/Inactive)

### 4. Service Master
- Service Code (auto-generated)
- Service Name
- Category (Consultation, Therapy, Investigation, Package)
- Description
- Default Rate
- Duration (in minutes)
- GST Percentage
- Active Status

### 5. Billing Module

#### Bill Generation
- Bill Number (auto-generated with prefix)
- Bill Date
- Patient Selection (UHID search)
- Service Selection (multiple)
- Quantity
- Rate
- Discount (% or amount)
- GST
- Total Amount
- Payment Mode (Cash, Card, UPI, Net Banking, Cheque, Mixed)
- Payment Details
- Due Amount
- Partial Payment tracking

#### Bill Operations
- **Bill Modification:** Allow editing within 24 hours of creation
- **Bill Cancel:** Mark as cancelled with reason, maintain audit trail
- **Bill Refund:** Process refunds with reason and approval
- **Bill Discount:** Apply percentage or fixed amount discount with reason

### 6. Patient Ledger
- Patient UHID
- Opening Balance
- All transactions (billing, payments, refunds, discounts)
- Running Balance
- Transaction Date
- Transaction Type (Debit/Credit)
- Reference (Bill No/Receipt No)
- Narration

### 7. Reports Module

#### Patient Report
- Date Range Filter
- Registration Type (New/Revisit)
- Status Filter
- Export to Excel/PDF
- Fields: UHID, Name, Mobile, DOB, Registration Date, Status

#### Billing Report
- Date Range Filter
- Payment Mode Filter
- Status Filter (Active/Cancelled)
- Summary and Detailed views
- Export options

#### Patient Ledger Report
- Patient-wise ledger
- Date Range Filter
- Transaction details
- Balance summary

#### Revenue Report
- Daily/Weekly/Monthly/Yearly
- By Service Category
- By Payment Mode
- Collection vs Due
- Profit Analysis

---

## Database Schema

### Tables

1. **ClinicProfiles** - Single row for clinic settings
2. **Users** - System users with roles
3. **Patients** - Patient demographics
4. **Services** - Service master
5. **Bills** - Bill headers
6. **BillDetails** - Individual line items
7. **BillPayments** - Payment transactions
8. **PatientLedgers** - All patient transactions
9. **BillModifications** - Audit trail for bill changes
10. **BillCancellations** - Cancellation records
11. **BillRefunds** - Refund records
12. **BillDiscounts** - Discount records

---

## UI/UX Design

### Color Scheme
- **Primary:** #2E7D32 (Green - representing health)
- **Secondary:** #1565C0 (Blue - professional)
- **Accent:** #FF6F00 (Orange - highlights)
- **Background:** #F5F5F5 (Light gray)
- **Text:** #212121 (Dark gray)
- **Success:** #4CAF50
- **Warning:** #FF9800
- **Error:** #F44336

### Layout
- Sidebar navigation with icons
- Top header with clinic name and user info
- Content area with cards and tables
- Modal dialogs for forms
- Toast notifications for actions

### Responsive Design
- Desktop-first approach
- Tablet support
- Mobile-friendly forms

---

## Security Features

- Password hashing with BCrypt
- Session management
- Role-based access control
- Audit logging
- Input validation
- SQL injection prevention (EF Core parameterized queries)
- XSS prevention

---

## Workflows

### Patient Registration Flow
1. Navigate to Patient > New Registration
2. Fill patient details
3. System generates UHID on save
4. Patient appears in patient list

### Billing Flow
1. Navigate to Billing > New Bill
2. Search and select patient
3. Add services
4. Apply discounts if needed
5. Select payment mode
6. Save bill
7. Generate receipt
8. Update patient ledger

### Bill Operations Flow
1. Search bill by number/date
2. Select operation (Modify/Cancel/Refund/Discount)
3. Provide reason
4. Confirm action
5. System updates bill and ledger

---

## Acceptance Criteria

1. ✅ User can login with valid credentials
2. ✅ UHID generated with configurable prefix on patient save
3. ✅ All billing payment modes functional
4. ✅ Patient ledger updates automatically on all transactions
5. ✅ User master allows CRUD operations with role assignment
6. ✅ Clinic profile affects billing header/footer/logo
7. ✅ Login page displays clinic logo and name
8. ✅ Service master allows service management
9. ✅ All reports generate with date filters
10. ✅ Bill modification works within time limit
11. ✅ Bill cancellation preserves audit trail
12. ✅ Bill refund updates ledger correctly
13. ✅ Bill discount applies with reason
14. ✅ All data persists in SQL Server
15. ✅ Responsive UI on all devices