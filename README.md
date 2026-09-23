# MediCamp

A comprehensive health camp management, clinical triage, and community health portal designed for NGOs, doctors, pharmacists, field volunteers, and patients to orchestrate and execute free medical camps.

---

## Table of Contents
- [Overview](#overview)
- [Key Features by Role](#key-features-by-role)
- [System Architecture & Tech Stack](#system-architecture--tech-stack)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Local Setup](#local-setup)
  - [Running with Docker](#running-with-docker)
- [Demo Credentials](#demo-credentials)
- [Project Structure](#project-structure)
- [Database & Seed Data](#database--seed-data)
- [Environment Configuration](#environment-configuration)
- [Global Analytics & Reporting](#global-analytics--reporting)

---

## Overview

MediCamp bridges the gap in humanitarian healthcare delivery by digitizing the entire lifecycle of temporary medical camps in remote and underserved communities. From camp approval, volunteer mobilization, and field triage to doctor consultations, digital prescriptions, medicine dispensing, and long-term health records, MediCamp provides an integrated, role-based platform.

---

## Key Features by Role

### 👨‍💼 1. System Administrator
- **Global Operations Dashboard:** Real-time KPI counters for active camps, registered beneficiaries, clinical consultations, and inventory movement.
- **NGO & Host Verification:** Review and approve/reject host organization registrations with official NGOAB registration tracking and audit notes.
- **Camp Directory Oversight:** Moderate, approve, and monitor public and active medical camps across the country.
- **Master Data Management:** Manage administrative divisions, districts, upazilas, standardized medicine catalogues, and partner hospital registries.
- **Global Analytics & Reports:** National epidemiological disease trends, demographic heatmaps, seasonal illness tracking, and A4 PDF-optimized printable executive reports.
- **User Management:** Activate, deactivate, and adjust user roles with full system auditability.

### 🏢 2. NGO / Camp Host
- **Camp Lifecycle Management:** Create, configure, publish, operate, and complete medical camps with venue, date, and specialization settings.
- **Staff Recruitment & Assignment:** Mobilize verified doctors, pharmacists, and field volunteers with real-time invitation and assignment tracking.
- **Camp Medicine Inventory:** Allocate medicines, set batch numbers, record expiry dates, and track real-time stock consumption.
- **Budgeting & Expense Tracking:** Log and categorize operational expenses (medicines, venue, logistics) with real-time budget variance tracking.
- **Camp Performance Analytics:** Camp-specific disease statistics, beneficiary demographics, and financial breakdown reports.

### 🩺 3. Doctor (Clinical Portal)
- **Real-Time Patient Queue:** Live waiting room queue displaying triage vitals and chief complaints in priority order.
- **Direct Triage Intake:** Fast-track consultation intake for direct patient arrivals.
- **Clinical Consultation Room:** Complete diagnostic interface with review of vital signs (BP, pulse, glucose, SpO2, BMI, temperature).
- **Digital Prescription Generator:** Structured prescription workflow with instant inventory lookup, dosage, frequency, duration, and patient instructions.
- **Non-Prescription Support:** Support for lifestyle/dietary advice and follow-up care ("Advice Only" visit tracking).
- **Tertiary Care Referrals:** Formal referral letters to registered hospitals for complex clinical escalations.
- **Longitudinal Medical History:** Access past camp visits, diagnoses, and medication history across all camps.

### 💊 4. Pharmacist (Dispensing Portal)
- **Live Dispensing Queue:** Real-time stream of issued prescriptions ready for verification and dispensing.
- **Multi-Row Stock Safety:** Aggregated quantity deduction to guarantee inventory integrity and prevent stock underflow.
- **Camp Inventory Dashboard:** Real-time stock levels, low-stock warnings, and batch tracking.
- **Dosage Verification & Fulfillment:** Mark prescriptions as dispensed with one-click verification.

### 🤝 5. Field Volunteer (Intake & Triage)
- **Rapid Patient Search:** Instant patient lookup by NID, phone number, or 6-character Unique Patient ID (`PatientUniqueId`).
- **On-the-Spot Registration:** Fast beneficiary registration and camp token generation for walk-in patients.
- **Field Vitals & Triage:** Record Blood Pressure, Pulse, Blood Glucose (fasting/random), Body Temperature, Respiratory Rate, SpO2, Weight, Height, and Chief Complaints.
- **Follow-up Management:** Schedule and track follow-up visits with Bangladesh timezone validation.

### 👤 6. Beneficiary / Patient
- **Personal Health Record:** Access past medical camp visits, diagnostic history, and doctor consultation notes.
- **Printable Prescriptions:** View and download clean, printable digital prescriptions.
- **Camp Discovery:** Search and discover upcoming free health camps filtered by division, district, and medical specialty.
- **Blood Donation Hub:**
  - Opt-in/out donor registry with blood group, district, and last donation tracking.
  - Public urgent blood request broadcaster (Emergency, Urgent, Routine).
  - Personal blood donation history logs and badges.

### 🔔 7. Real-Time Notification System
- Dynamic in-app notification center for staff assignments, camp approvals, blood requests, and critical alerts.

---

## System Architecture & Tech Stack

- **Backend:** ASP.NET Core MVC 10.0 (C# 13)
- **Database:** PostgreSQL via **Entity Framework Core 10** (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Authentication:** ASP.NET Core Cookie Authentication with role-based authorization policies
- **Frontend:** Razor Views, HTML5, CSS3, Vanilla JavaScript, Bootstrap 5, FontAwesome / Bootstrap Icons
- **Reporting Engine:** Custom responsive reporting layouts with `@media print` A4 PDF layout rendering
- **Containerization:** Multi-stage Dockerfile based on official Microsoft .NET 10 SDK & ASP.NET runtime images

---

## Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or newer
- [PostgreSQL](https://www.postgresql.org/) database instance (e.g., local PostgreSQL, Supabase, Neon, or Docker)

### Local Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/PanthaProtick/MediCamp.git
   cd MediCamp
   ```

2. **Configure Connection String:**
   Update `appsettings.json` or `appsettings.Development.json` with your PostgreSQL connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=medicamp_db;Username=postgres;Password=your_password;SSL Mode=Prefer;"
     }
   }
   ```

3. **Restore Dependencies & Run:**
   ```bash
   dotnet restore
   dotnet run
   ```
   > The application automatically initializes and seeds the database on first run via `DbSeeder`.

4. **Open in Browser:**
   Navigate to `http://localhost:5000` (or `https://localhost:5001`).

---

### Running with Docker

1. **Build the Docker image:**
   ```bash
   docker build -t medicamp:latest .
   ```

2. **Run the container:**
   ```bash
   docker run -d -p 8080:8080 \
     -e ConnectionStrings__DefaultConnection="Host=your_pg_host;Port=5432;Database=medicamp_db;Username=postgres;Password=your_password;SSL Mode=Require;" \
     --name medicamp-app medicamp:latest
   ```

3. **Access the application:**
   Open `http://localhost:8080` in your browser.

---

## Demo Credentials

The database is pre-seeded with ready-to-test accounts across all system roles:

| Role | Email | Password | Details |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@medicamp.org` | `Admin@123` | Central administrator with full operational & analytics access |
| **Host (NGO)** | `host@brachospital.org` | `Host@123` | Approved NGO camp coordinator (BRAC Health Initiative) |
| **Doctor** | `doctor.rahman@medicamp.org` | `Doctor@123` | Internal Medicine Specialist (BMDC verified) |
| **Pharmacist** | `pharma.poly@medicamp.org` | `Pharma@123` | Certified Camp Pharmacist |
| **Volunteer** | `volunteer.karim@medicamp.org` | `Volunteer@123` | Field triage and patient registration volunteer |
| **Patient** | `patient.anwar@medicamp.org` | `Patient@123` | Registered beneficiary with visit history (ID: `A3K9P2`) |

---

## Project Structure

```text
MediCamp/
├── Controllers/              # MVC controllers handling business workflows
│   ├── AccountController.cs       # Authentication, registration, and user profiles
│   ├── AdminController.cs         # Admin oversight, NGO approval, and global analytics
│   ├── DoctorController.cs        # Clinical triage queue, consultations, and prescriptions
│   ├── HomeController.cs          # Public landing page, camp directory, and blood ticker
│   ├── HostController.cs          # Camp creation, staff assignment, inventory, and expenses
│   ├── MasterDataController.cs    # Geolocation, medicine catalog, and hospital endpoints
│   ├── NotificationController.cs  # In-app notifications and alert dispatching
│   ├── PatientController.cs       # Beneficiary portal, history, and Blood Donation Hub
│   ├── PharmacistController.cs    # Prescription dispensing queue and camp inventory
│   └── VolunteerController.cs     # Patient search, camp check-in, and triage vitals
├── Data/                     # EF Core DbContext and database seeders
│   ├── ApplicationDbContext.cs    # PostgreSQL DbContext configuration and relations
│   └── DbSeeder.cs                # Automatic schema validation and comprehensive demo datasets
├── Models/                   # Entity models, view models, and domain enumerations
│   ├── Domain/                    # Core business entities (Camp, Prescription, Triage, etc.)
│   └── ViewModels/                # Role-tailored view models for UI data binding
├── Services/                 # Business logic and EF Core-backed data services
│   ├── IMockDataService.cs        # Service interface
│   └── MockDataService.cs         # EF Core service implementation for users, camps, and operations
├── Views/                    # Role-specific Razor view templates
│   ├── Account/                   # Login, register, profile, and security views
│   ├── Admin/                     # Global dashboards, NGO approvals, and analytics reports
│   ├── Doctor/                    # Consultation room, triage queue, and referral sheets
│   ├── Home/                      # Public portal, active camp search, and blood hub
│   ├── Host/                      # Camp management, staff recruitment, and budget tracking
│   ├── Patient/                   # Medical history, prescription viewer, and donor registry
│   ├── Pharmacist/                # Dispensing queue and real-time inventory management
│   ├── Volunteer/                 # Field intake, vitals recording, and follow-up scheduling
│   └── Shared/                    # Master layouts, role navigation bars, and notification modals
├── wwwroot/                  # Static assets (CSS stylesheets, JavaScript modules, vendor assets)
├── Dockerfile                # Production multi-stage Docker build configuration
├── Program.cs                # ASP.NET Core pipeline, middleware, and dependency injection setup
└── appsettings.json          # Application configuration settings
```

---

## Database & Seed Data

MediCamp utilizes **Entity Framework Core** with **PostgreSQL**.
On application startup, `DbSeeder.SeedData()` automatically:
1. Validates and ensures the existence of all required schema tables (including blood requests, donation logs, camp registrations, and expenses).
2. Upserts initial master data (Bangladesh divisions, districts, upazilas, medicine database, hospitals, and blood compatibility data).
3. Populates realistic demo records including active medical camps, assigned doctors, triage vitals, digital prescriptions, and blood requests.

---

## Environment Configuration

You can configure the application via `appsettings.json` or standard environment variables:

| Variable | Description | Default / Example |
| :--- | :--- | :--- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;Database=medicamp_db;Username=postgres;Password=...` |
| `ASPNETCORE_ENVIRONMENT` | Application runtime environment | `Development` or `Production` |
| `ASPNETCORE_URLS` | Binding URLs and ports | `http://0.0.0.0:8080` (Docker default) |

---

## Global Analytics & Reporting

MediCamp includes an integrated analytics engine accessible to Administrators (`/Admin/Reports`) and Camp Hosts (`/Host/Reports`):
- **Epidemiological Insights:** Top diagnosed conditions, disease category distributions, and seasonal spikes.
- **Geographic Drill-Down:** Disease prevalence filterable by Division, District, and Upazila.
- **Financial & Operational Audits:** Camp expense breakdowns, medicine allocation vs. consumption rates, and patient turnout.
- **Export to PDF:** Built-in CSS print styling formatted for standard A4 paper export with high-contrast print headers and tables.