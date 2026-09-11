# MediCamp

A comprehensive health camp management and clinical triage system designed for NGOs, doctors, volunteers, and pharmacists to orchestrate and manage free medical camps.

## Table of Contents
- [Overview](#overview)
- [Current Progress](#current-progress)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Environment Variables](#environment-variables)
- [Team & Ownership](#team--ownership)
- [Design System](#design-system)
- [Known Issues](#known-issues)
- [Roadmap](#roadmap)
- [Deployment](#deployment)
- [License](#license)

---

## Overview
MediCamp is a role-based web application tailored to streamline the execution of medical camps. It provides dedicated portals for:
- **Patients:** Register, view history, and opt-in/out of a centralized blood donation hub.
- **NGO / Hosts:** Setup camps, manage staffing, allocate inventory, and monitor camp metrics.
- **Volunteers:** Handle patient search, registration, and triage vitals on the field.
- **Doctors:** Access patient queues, review clinical history, and issue digital prescriptions.
- **Pharmacists:** View live prescription queues, manage camp inventory, and dispense medicine.
- **Administrators:** Oversee global operations, approve NGOs, and manage master system data.

---

## Current Progress
**Phases 1–7 are currently implemented.**
- Role-based authentication (Admin, Host, Doctor, Volunteer, Pharmacist, Patient) is fully implemented using ASP.NET Core Cookie Authentication.
- Registration flows tailored for specific roles (e.g., Doctors requiring BMDC numbers, Hosts requiring NGO licenses) are functional.
- EF Core with PostgreSQL persistence is implemented for users, camps, staffing, triage, consultations, prescriptions, inventory, follow-ups, and blood donation workflows.
- Patient medical history, digital prescriptions, volunteer follow-up management, and the blood donation hub are implemented.
- Frontend layouts and controllers are organized by role and workflow.

---

## Tech Stack
- **Framework:** ASP.NET Core MVC (v10.0)
- **Language:** C#
- **Frontend:** HTML5, CSS3, Razor Views, Bootstrap 5
- **Data Access:** Entity Framework Core with Npgsql (PostgreSQL)
- **Supporting Service:** `IMockDataService` remains for selected staffing and account workflows.

---

## Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or newer
- Visual Studio 2022, JetBrains Rider, or VS Code

---

## Getting Started
1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd MediCamp
   ```
2. **Run the application:**
   ```bash
   dotnet run
   ```
3. **Access the Web App:** Open `http://localhost:5000` (or `https://localhost:5001`) in your browser.
4. **Log in with Mock Credentials:**
   - **Admin:** `admin@medicamp.org` (Pass: `Admin@123`)
   - **Host:** `host@brachospital.org` (Pass: `Host@123`)
   - **Doctor:** `doctor.rahman@medicamp.org` (Pass: `Doctor@123`)
   - **Pharmacist:** `pharma.poly@medicamp.org` (Pass: `Pharma@123`)

---

## Project Structure
```text
MediCamp/
├── Controllers/       # Handles incoming HTTP requests (Account, Admin, Home)
├── Models/            # Domain models, ViewModels, and ApplicationUser
├── Services/          # Business logic & data access (MockDataService currently)
├── Views/             # Razor pages organized by feature (Account, Admin, Shared)
├── wwwroot/           # Static web assets (CSS, JS, Images)
├── Program.cs         # App configuration and dependency injection
└── appsettings.json   # Application settings
```

---

## Environment Variables
The application requires a PostgreSQL connection configured as `ConnectionStrings__DefaultConnection` or in `appsettings.json`.
*Optional deployment configuration includes:*
- `ConnectionStrings__DefaultConnection` (PostgreSQL Database URL)
- `JWT_Secret` or specific cookie security salts if applicable.

---

## Team & Ownership
- **Maintainer:** AkifFarhan
- *To add additional team members, update this section.*

---

## Design System
The frontend utilizes **ASP.NET Core Razor Pages** built primarily with standard HTML forms and **Bootstrap** utilities to ensure a responsive, clean, and accessible UI across desktop and mobile (essential for field volunteers).

---

## Known Issues
- **Mixed service boundaries:** Selected staffing and account operations still use `IMockDataService` while core clinical workflows use EF Core.
- **Reporting pending:** Phase 8 reporting and analytics dashboards remain to be implemented.

---

## Roadmap
The system is being built across 8 distinct phases:
1. **[Completed] Foundation & Auth:** Login, registration, and role assignment.
2. **[Completed] Master Data:** Locations, medicines, hospitals, and blood groups.
3. **[Completed] Camp Management:** NGO camp setup, approval, and public directory.
4. **[Completed] Field Staffing & Inventory:** Staff requests and medicine allocation.
5. **[Completed] Field Operations (Triage):** Patient entry, vitals, and token generation.
6. **[Completed] Clinical Workflow:** Doctor consultation, prescriptions, referrals, and dispensing.
7. **[Completed] Patient Portal:** Medical history, follow-ups, and blood donation hub.
8. **[Pending] Reporting:** Analytics and monitoring dashboards for Admins/NGOs.

---

## Deployment
*Deployment targets are planned but not yet configured.*
- **Containerization:** Docker support is planned.
- **Cloud Host:** Azure App Service or Render.
- **Database:** Supabase or Neon (PostgreSQL).

---

## License
This project is proprietary. All rights reserved.