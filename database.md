# Database Setup Guide (PostgreSQL via Neon)

This guide provides step-by-step instructions for transitioning the MediCamp project from the In-Memory `MockDataService` to a real **PostgreSQL** database hosted on **Neon** using **Entity Framework Core**.

---

## 1. Setting up Neon (PostgreSQL Host)
[Neon](https://neon.tech/) is a serverless Postgres database. 
1. **Create an account:** Go to [Neon.tech](https://neon.tech/) and sign up.
2. **Create a project:** Click "New Project", name it `MediCampDB`, select your preferred region, and choose Postgres version 16 (or latest).
3. **Get the Connection String:** On your project dashboard, find the **Connection Details** section. Copy the provided connection string. It will look like this:
   `postgres://[user]:[password]@[endpoint].neon.tech/neondb?sslmode=require`

---

## 2. Installing Entity Framework Core Packages
Open your terminal in the MediCamp project directory and install the necessary NuGet packages for Entity Framework Core and PostgreSQL:

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

---

## 3. Configuring `appsettings.json`
Add your Neon connection string to the `appsettings.json` file. Replace the `<Your_Neon_Connection_String>` placeholder with your actual connection string.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "<Your_Neon_Connection_String>"
  }
}
```

---

## 4. Creating the `ApplicationDbContext`
Create a new file in a new `Data` folder (`Data/ApplicationDbContext.cs`) to act as the bridge between your C# models and the PostgreSQL database.

```csharp
using MediCamp.Models;
using MediCamp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace MediCamp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Camp> Camps { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<MasterMedicine> MasterMedicines { get; set; }
        public DbSet<BloodDonationProfile> BloodDonationProfiles { get; set; }
        public DbSet<CampInventory> CampInventories { get; set; }
        public DbSet<TriageRecord> TriageRecords { get; set; }
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<Referral> Referrals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Define specific relationships or constraints if necessary here.
        }
    }
}
```

---

## 5. Updating `Program.cs`
Update `Program.cs` to register the `ApplicationDbContext` and configure it to use Npgsql (PostgreSQL).

```csharp
// Add this to the top:
// using MediCamp.Data;
// using Microsoft.EntityFrameworkCore;

// Replace the IMockDataService line with:
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Note: You will eventually need to refactor AccountController to use DbContext instead of IMockDataService.
```

---

## 6. Running Migrations
To create the database tables in Neon, run the Entity Framework Core migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## 7. Database Schema Reference

Here is the exact schema and data types mapped from the application models to PostgreSQL.

### Table: `Users` (Mapped from `ApplicationUser`)
Handles authentication and profile information for all roles (Patients, Doctors, Hosts, etc.).
- `Id` (text, Primary Key) - GUID
- `FullName` (character varying 100, Required)
- `Email` (text, Required, unique constraint recommended)
- `PhoneNumber` (text, Required)
- `NID` (character varying 20, Nullable)
- `DateOfBirth` (timestamp without time zone, Nullable)
- `Gender` (character varying 15)
- `BloodGroup` (character varying 10)
- `District` (character varying 50)
- `Upazila` (character varying 50)
- `Address` (character varying 250)
- `Role` (text) - e.g. "Admin", "Host", "Doctor", "Patient"
- `OrganizationName` (character varying 150, for Hosts)
- `OrganizationRegNo` (character varying 50, for Hosts)
- `FocalPersonContact` (character varying 50, for Hosts)
- `HostApprovalStatus` (character varying 20) - Default "Approved"
- `HostRejectionReason` (text)
- `MedicalSpecialization` (character varying 100, for Doctors)
- `BMDCRegNo` (character varying 50, for Doctors)
- `IsActive` (boolean)
- `CreatedAt` (timestamp without time zone)
- `LastLoginAt` (timestamp without time zone)
- `PasswordHash` (text)

### Table: `Camps`
Stores information about medical camps organized by Hosts.
- `Id` (integer, Primary Key, Auto-increment)
- `Title` (character varying 150, Required)
- `CampType` (character varying 100, Required) - e.g. "Eye Camp"
- `District` (character varying 50, Required)
- `Upazila` (character varying 50, Required)
- `Venue` (character varying 150)
- `StartDate` (timestamp without time zone)
- `EndDate` (timestamp without time zone)
- `ExpectedPatients` (integer)
- `RegisteredPatientsCount` (integer)
- `ServedPatientsCount` (integer)
- `TotalBudget` (numeric(18,2))
- `UtilizedBudget` (numeric(18,2))
- `Status` (character varying 20) - "Scheduled", "Ongoing", "Completed"
- `HostId` (text, Foreign Key -> `Users.Id`)
- `Description` (character varying 500)
- `CreatedAt` (timestamp without time zone)

### Table: `Locations`
Master data for Bangladesh geographical locations.
- `Id` (integer, Primary Key, Auto-increment)
- `Division` (character varying 50, Required)
- `District` (character varying 50, Required)
- `Upazila` (character varying 50, Required)
- `Union` (character varying 50, Nullable)
- `Village` (character varying 50, Nullable)

### Table: `MasterMedicines`
Master inventory dictionary for prescribing and tracking medicines.
- `Id` (integer, Primary Key, Auto-increment)
- `BrandName` (character varying 100, Required)
- `GenericName` (character varying 100, Required)
- `DosageForm` (character varying 50) - e.g., "Tablet", "Syrup"
- `Strength` (character varying 50) - e.g., "500mg"
- `Category` (character varying 50) - e.g., "Antibiotic"
- `IsEssential` (boolean) - Default true

### Table: `BloodDonationProfiles`
Stores details of users opted into the blood donation pool.
- `Id` (integer, Primary Key, Auto-increment)
- `UserId` (text, Required, Foreign Key -> `Users.Id`)
- `IsAvailableDonor` (boolean)
- `BloodGroup` (character varying 10)
- `District` (character varying 50)
- `Upazila` (character varying 50)
- `LastDonatedDate` (timestamp without time zone, Nullable)
- `TotalDonationsCount` (integer) - Default 0
- `WeightKg` (double precision, Nullable)

### Table: `CampInventories`
Tracks stock allocated by Hosts for specific camps and dispensing progress.
- `Id` (integer, Primary Key)
- `CampId` (integer, Foreign Key -> `Camps.Id`)
- `MasterMedicineId` (integer, Foreign Key -> `MasterMedicines.Id`)
- `QuantityAllocated` (integer)
- `QuantityDispensed` (integer) - Default 0

### Table: `TriageRecords`
Logged by Volunteers when a patient arrives at a camp.
- `Id` (integer, Primary Key)
- `CampId` (integer, Foreign Key -> `Camps.Id`)
- `PatientId` (text, Foreign Key -> `Users.Id`)
- `VolunteerId` (text, Foreign Key -> `Users.Id`)
- `RecordedAt` (timestamp without time zone)
- `BloodPressure` (character varying 20, Nullable)
- `TemperatureF` (double precision, Nullable)
- `WeightKg` (double precision, Nullable)
- `HeightCm` (double precision, Nullable)
- `BMI` (double precision, Nullable)
- `PresentingSymptoms` (character varying 500, Nullable)
- `UrgencyLevel` (character varying 20) - e.g. "Normal", "Urgent"
- `TokenNumber` (integer)
- `IsSeenByDoctor` (boolean)

### Table: `Consultations`
Created by Doctors upon reviewing a patient's TriageRecord.
- `Id` (integer, Primary Key)
- `TriageRecordId` (integer, Foreign Key -> `TriageRecords.Id`)
- `DoctorId` (text, Foreign Key -> `Users.Id`)
- `ConsultedAt` (timestamp without time zone)
- `ClinicalNotes` (character varying 500, Nullable)
- `Diagnosis` (character varying 250, Nullable)
- `Advice` (character varying 500, Nullable)

### Table: `Prescriptions`
Parent record for prescribed medicines, linked to a consultation.
- `Id` (integer, Primary Key)
- `ConsultationId` (integer, Foreign Key -> `Consultations.Id`)
- `IsDispensed` (boolean)
- `DispensedByPharmacistId` (text, Nullable, Foreign Key -> `Users.Id`)
- `DispensedAt` (timestamp without time zone, Nullable)

### Table: `PrescriptionItems`
Individual medicines prescribed to the patient.
- `Id` (integer, Primary Key)
- `PrescriptionId` (integer, Foreign Key -> `Prescriptions.Id`)
- `MasterMedicineId` (integer, Foreign Key -> `MasterMedicines.Id`)
- `Dosage` (character varying 50) - e.g. "1+0+1"
- `DurationDays` (integer)
- `Instructions` (character varying 150, Nullable)
- `QuantityPrescribed` (integer)
- `QuantityDispensed` (integer)

### Table: `Referrals`
For patients needing advanced care outside the camp.
- `Id` (integer, Primary Key)
- `ConsultationId` (integer, Foreign Key -> `Consultations.Id`)
- `ReferredHospital` (character varying 150, Required)
- `Reason` (character varying 500, Nullable)
- `Urgency` (character varying 20) - e.g. "Routine", "Urgent"

---

### Table: `CampStaffRequests`
Tracks participation requests sent by Hosts to Doctors for a specific camp.
- `Id` (integer, Primary Key, Auto-increment)
- `CampId` (integer, Required, Foreign Key -> `Camps.Id`)
- `DoctorId` (text, Required, Foreign Key -> `Users.Id`)
- `Status` (character varying 20) - "Pending", "Approved", "Denied"
- `RequestedAt` (timestamp without time zone)
- `RespondedAt` (timestamp without time zone, Nullable)

---

## Next Steps
Once the database is set up and the tables are generated via migrations, the final step is to rewrite the `AccountController` and authentication flows to save and query users directly from `ApplicationDbContext` instead of the `MockDataService`.
