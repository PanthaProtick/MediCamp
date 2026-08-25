# MediCamp Proposed Pages & Content

Based on the requirements, MediCamp will utilize a role-based architecture. The application will be divided into specific areas for each of the six actors, plus public-facing pages.

## 1. Public & Authentication Pages
These pages are accessible without logging in, or are used to authenticate users.

*   **Landing Page (Home)**
    *   *Content:* Introduction to MediCamp, mission statement, impact statistics (e.g., total camps, patients served), call-to-action for NGOs to register, and a link for patients to find upcoming camps.
*   **Authentication (Login / Register)**
    *   *Content:* Login form (Email/NID and password). Registration options for Patients and Hosts (NGOs). *Note: Doctors, Volunteers, and Pharmacists are typically created or invited by Admins/Hosts.*
*   **Public Camp Directory**
    *   *Content:* Searchable and filterable list of upcoming and active camps (by district, upazila, date).

---

## 2. Admin Dashboard
Accessible only to System Administrators. Focuses on system-wide configuration and oversight.

*   **Admin Overview (Dashboard)**
    *   *Content:* High-level metrics (total active camps, pending NGO approvals, total users by role).
*   **Host (NGO) Approvals**
    *   *Content:* List of pending registration requests from NGOs with options to review details, approve, or reject.
*   **User Management**
    *   *Content:* CRUD interface for all system users. Ability to reset passwords, change roles, and disable accounts.
*   **Master Data Management**
    *   *Content:* Interfaces to manage reference tables: Locations (District/Upazila/Union/Village), Master Medicine List, Hospital List, and Blood Groups.
*   **Global Reports & Analytics**
    *   *Content:* System-wide reports across all Hosts. Disease prevalence maps, global inventory usage, and demographic data.

---

## 3. Host (NGO) Dashboard
Accessible to NGO organizers. Focuses on planning, staffing, and monitoring specific camps.

*   **Host Overview**
    *   *Content:* Summary of the NGO's camps (upcoming, active, completed), budget utilization summary, and active restock alerts.
*   **Camp Management (List & Setup)**
    *   *Content:* Form to create a new camp (Name, Type, Location, Dates, Expected Patients, Total Budget). List of all camps owned by the Host.
*   **Camp Details & Staffing**
    *   *Content:* For a specific camp: UI to assign specific Doctors, Volunteers, and Pharmacists from the user pool.
*   **Camp Inventory Allocation**
    *   *Content:* Interface to allocate medicines and equipment from the NGO's central stock to a specific camp.
*   **Camp Monitoring & Financials**
    *   *Content:* Real-time view of camp progress (patients registered vs. seen). Form to log expenses against the camp budget.
*   **Host Reports**
    *   *Content:* Reports scoped only to this NGO's camps (Financial reports, staff performance, patient demographics).

---

## 4. Volunteer Dashboard
Accessible to Volunteers. Focuses on fast data entry in the field.

*   **Active Camp Selection**
    *   *Content:* A simple screen to select which active camp the volunteer is currently working at.
*   **Patient Search & Registration**
    *   *Content:* Search bar (by NID, Phone, or Name). Form to register a new patient (Basic info, demographic data).
*   **Triage & Token Generation**
    *   *Content:* Form to record vitals (Blood Pressure, BMI, Temperature) and presenting symptoms. Button to generate a queue token and add the patient to the Doctor's queue.
*   **Follow-up Management**
    *   *Content:* List of patients requiring follow-ups from previous visits. Interface to log contact attempts and record recovery status.

---

## 5. Doctor Dashboard
Accessible to Doctors. Focuses on clinical history and consultation.

*   **Patient Queue**
    *   *Content:* Live, auto-updating list of patients waiting to be seen, ordered by token number or triage urgency.
*   **Consultation Workspace**
    *   *Content:* The main clinical screen.
        *   *Left Panel:* Patient's longitudinal history (past visits, chronic conditions, allergies across all camps).
        *   *Right Panel:* Forms to enter current diagnosis, clinical notes, and advice.
*   **Prescription & Referral Builder**
    *   *Content:* Interface to select medicines from the camp's allocated inventory (shows current stock). Form to set dosage and frequency. Section to create a hospital referral with urgency flags.

---

## 6. Pharmacist Dashboard
Accessible to Pharmacists. Focuses on dispensing and inventory control.

*   **Prescription Queue**
    *   *Content:* Live list of prescriptions sent by the Doctors for the current camp.
*   **Dispensing Interface**
    *   *Content:* View of the selected prescription. Checkboxes to mark items as dispensed (triggers auto-deduction from stock). Ability to substitute out-of-stock items with alternatives.
*   **Inventory & Restock**
    *   *Content:* Current stock levels for the active camp. Alerts for low stock. Button to send a restock request to the Host.

---

## 7. Patient Portal & Blood Donation
Accessible to registered Patients.

*   **My Profile & History**
    *   *Content:* Patient's personal details. Read-only timeline of their past camp visits, diagnoses, and digital prescriptions.
*   **Blood Donation Hub**
    *   *Content:*
        *   Toggle to opt-in/out as a blood donor.
        *   Form to submit a blood request (Group, Units, Urgency, Hospital).
        *   Search interface to find donors by blood group and location.
        *   Log of past donations/requests.
