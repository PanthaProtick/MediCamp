using System.ComponentModel.DataAnnotations;
using MediCamp.Models.Domain;

namespace MediCamp.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your Email Address or National ID (NID).")]
        [Display(Name = "Email Address or NID")]
        public string Identifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please select a role to register as.")]
        [Display(Name = "Account Role")]
        public string Role { get; set; } = SystemRoles.Patient; // Patient, Host, Doctor, Volunteer, Pharmacist

        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Valid Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number (e.g., 017xxxxxxxx).")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "National ID (NID)")]
        public string? NID { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public string Gender { get; set; } = "Male";

        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; } = "O+";

        [Required(ErrorMessage = "District is required.")]
        [Display(Name = "District")]
        public string District { get; set; } = "Dhaka";

        [Required(ErrorMessage = "Upazila is required.")]
        [Display(Name = "Upazila")]
        public string Upazila { get; set; } = "Dhanmondi";

        [Required(ErrorMessage = "Address is required.")]
        [Display(Name = "Village / Street Address")]
        public string Address { get; set; } = string.Empty;

        // Doctor Specific
        [Display(Name = "Medical Specialization / Degrees")]
        public string? MedicalSpecialization { get; set; }

        [Display(Name = "BMDC Registration Number")]
        public string? BMDCRegNo { get; set; }

        // Host Specific
        [Display(Name = "Organization Name")]
        public string? OrganizationName { get; set; }

        [Display(Name = "Organization Type")]
        public string OrganizationType { get; set; } = "NGO"; // NGO, Hospital, Corporate, Community Group, Other

        [Display(Name = "Registration / License No. (Optional)")]
        public string? OrganizationRegNo { get; set; }

        [Display(Name = "Opt-in as Voluntary Blood Donor in MediCamp Network")]
        public bool IsBloodDonor { get; set; } = true;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class EditProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "National ID (NID)")]
        public string? NID { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; }

        [Display(Name = "District")]
        public string? District { get; set; }

        [Display(Name = "Upazila")]
        public string? Upazila { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        // Specific fields
        [Display(Name = "Medical Specialization / Degrees")]
        public string? MedicalSpecialization { get; set; }

        [Display(Name = "BMDC Registration Number")]
        public string? BMDCRegNo { get; set; }

        [Display(Name = "Organization Name")]
        public string? OrganizationName { get; set; }

        [Display(Name = "Organization Type")]
        public string? OrganizationType { get; set; }

        [Display(Name = "Registration / License No")]
        public string? OrganizationRegNo { get; set; }

        [Display(Name = "Focal Person Contact")]
        public string? FocalPersonContact { get; set; }
    }

    public class RegisterPatientViewModel : RegisterViewModel
    {
        public RegisterPatientViewModel()
        {
            Role = SystemRoles.Patient;
        }
    }

    public class RegisterHostViewModel : RegisterViewModel
    {
        public RegisterHostViewModel()
        {
            Role = SystemRoles.Host;
        }

        public string ContactPersonName
        {
            get => FullName;
            set => FullName = value;
        }

        public string HeadOfficeAddress
        {
            get => Address;
            set => Address = value;
        }

        public string OperatingDistricts { get; set; } = "Kurigram, Sunamganj, Bandarban";
    }

    public class HostApprovalsViewModel
    {
        public List<ApplicationUser> PendingHosts { get; set; } = new();
        public List<ApplicationUser> ApprovedHosts { get; set; } = new();
        public List<ApplicationUser> RejectedHosts { get; set; } = new();
        public List<ApplicationUser> AllHosts { get; set; } = new();
        public string ActiveTab { get; set; } = "Pending";
        public string? SearchTerm { get; set; }

        public int PendingCount => PendingHosts.Count;
        public int ApprovedCount => ApprovedHosts.Count;
        public int RejectedCount => RejectedHosts.Count;
        public int TotalCount => AllHosts.Count;
    }

    public class UserManagementViewModel
    {
        public List<ApplicationUser> Users { get; set; } = new();

        public string? SearchTerm { get; set; }
        public string? SelectedRole { get; set; }
        public string? SelectedStatus { get; set; }

        public int TotalUsersCount { get; set; }
        public int ActiveAdminsCount { get; set; }
        public int ActiveHostsCount { get; set; }
        public int ActiveDoctorsCount { get; set; }
        public int ActiveVolunteersCount { get; set; }
        public int ActivePharmacistsCount { get; set; }
        public int ActivePatientsCount { get; set; }
        public int PendingApprovalsCount { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "National ID (NID)")]
        public string? NID { get; set; }

        [Required(ErrorMessage = "Please assign a system role.")]
        [Display(Name = "System Role")]
        public string Role { get; set; } = SystemRoles.Volunteer;

        [Display(Name = "District")]
        public string District { get; set; } = "Dhaka";

        [Display(Name = "Upazila")]
        public string Upazila { get; set; } = "Dhanmondi";

        [Display(Name = "Medical Specialization (Doctors Only)")]
        public string? MedicalSpecialization { get; set; }

        [Display(Name = "BMDC Reg No (Doctors Only)")]
        public string? BMDCRegNo { get; set; }

        [Display(Name = "Organization Name (NGO Hosts Only)")]
        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "Temporary Password is required.")]
        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "Temporary Password")]
        public string TemporaryPassword { get; set; } = "Pass@123";
    }

    public class HomeLandingViewModel
    {
        public int TotalCampsCount { get; set; }
        public int TotalPatientsServed { get; set; }
        public int TotalDoctorsCount { get; set; }
        public int TotalVolunteersCount { get; set; }
        public int FreeMedicinesDispensed { get; set; }
        public int DistrictsReached { get; set; }
        public List<CampOverviewItem> UpcomingCamps { get; set; } = new();
    }

    public class CampOverviewItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CampType { get; set; } = "General Healthcare";
        public string District { get; set; } = string.Empty;
        public string Upazila { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public string HostOrganization { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ExpectedPatients { get; set; }
        public int ServedPatientsCount { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled, Ongoing, Completed
        public string BadgeClass => Status switch
        {
            "Ongoing" => "bg-success",
            "Scheduled" => "bg-primary",
            "Completed" => "bg-secondary",
            _ => "bg-info"
        };
    }

    public class CreateCampViewModel
    {
        [Required(ErrorMessage = "Camp Title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        [Display(Name = "Camp Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a Camp Type.")]
        [Display(Name = "Camp Type / Specialty")]
        public string CampType { get; set; } = "General Healthcare & Triage";

        [Required(ErrorMessage = "Please select a District.")]
        [Display(Name = "Target District")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an Upazila.")]
        [Display(Name = "Target Upazila / Sub-district")]
        public string Upazila { get; set; } = string.Empty;

        [Required(ErrorMessage = "Venue name or address is required.")]
        [StringLength(150, ErrorMessage = "Venue cannot exceed 150 characters.")]
        [Display(Name = "Specific Venue / Field Site")]
        public string Venue { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Camp Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "End Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Camp End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(3);

        [Required(ErrorMessage = "Expected Patient Capacity is required.")]
        [Range(1, 100000, ErrorMessage = "Expected capacity must be between 1 and 100,000 patients.")]
        [Display(Name = "Expected Patient Capacity")]
        public int ExpectedPatients { get; set; } = 500;

        [Required(ErrorMessage = "Total Estimated Budget is required.")]
        [Range(0, 100000000, ErrorMessage = "Budget must be a valid positive amount.")]
        [Display(Name = "Total Estimated Budget (BDT)")]
        public decimal TotalBudget { get; set; } = 100000.00m;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Camp Objectives & Target Community Needs")]
        public string? Description { get; set; }

        // Dropdown Sources (Dynamically Populated)
        public List<string> AvailableCampTypes { get; set; } = new();
        public List<string> AvailableDistricts { get; set; } = new();
        public List<string> AvailableUpazilas { get; set; } = new();
    }

    public class HostDashboardViewModel
    {
        public ApplicationUser HostUser { get; set; } = new();
        public List<Camp> MyCamps { get; set; } = new();
        
        public int TotalCampsCount => MyCamps.Count;
        public int PendingCount => MyCamps.Count(c => c.Status == "Pending Admin Approval");
        public int ScheduledCount => MyCamps.Count(c => c.Status == "Scheduled");
        public int OngoingCount => MyCamps.Count(c => c.Status == "Ongoing");
        public int CompletedCount => MyCamps.Count(c => c.Status == "Completed");
        
        public bool IsApproved => HostUser.HostApprovalStatus == "Approved";
    }
    public class AdminCampApprovalsViewModel
    {
        public string ActiveTab { get; set; } = "Pending";
        public List<AdminCampApprovalItemViewModel> PendingCamps { get; set; } = new();
        public List<AdminCampApprovalItemViewModel> ScheduledCamps { get; set; } = new();
        public List<AdminCampApprovalItemViewModel> RejectedCamps { get; set; } = new();
    }

    public class AdminCampApprovalItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CampType { get; set; } = string.Empty;
        public string HostOrganizationName { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Upazila { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ExpectedPatients { get; set; }
        public decimal TotalBudget { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string? CampRejectionReason { get; set; }

        // Automated Checks
        public bool HasDateConflict { get; set; }
        public string? ConflictWarningMessage { get; set; }
        public bool HasCapacitySanityFlag { get; set; }
        public string? CapacitySanityMessage { get; set; }
    }

    public class HostManageStaffViewModel
    {
        public Camp Camp { get; set; } = new();
        public List<CampStaffRequest> CurrentRequests { get; set; } = new();
        public List<ApplicationUser> AvailableDoctors { get; set; } = new();
        public List<CampVolunteerRequest> CurrentVolunteerRequests { get; set; } = new();
        public List<ApplicationUser> AvailableVolunteers { get; set; } = new();
        public List<CampPharmacistRequest> CurrentPharmacistRequests { get; set; } = new();
        public List<ApplicationUser> AvailablePharmacists { get; set; } = new();
        
        // Pagination properties
        public int CurrentDoctorPage { get; set; } = 1;
        public int TotalDoctorPages { get; set; } = 1;
        public int DoctorPageSize { get; set; } = 5;

        public int CurrentVolunteerPage { get; set; } = 1;
        public int TotalVolunteerPages { get; set; } = 1;
        public int VolunteerPageSize { get; set; } = 5;

        public int CurrentPharmacistPage { get; set; } = 1;
        public int TotalPharmacistPages { get; set; } = 1;
        public int PharmacistPageSize { get; set; } = 5;
    }

    public class DoctorRequestsViewModel
    {
        public List<CampStaffRequest> PendingRequests { get; set; } = new();
        public List<CampStaffRequest> RespondedRequests { get; set; } = new();
    }

    public class VolunteerRequestsViewModel
    {
        public List<CampVolunteerRequest> PendingRequests { get; set; } = new();
        public List<CampVolunteerRequest> RespondedRequests { get; set; } = new();
    }

    public class PharmacistRequestsViewModel
    {
        public List<CampPharmacistRequest> PendingRequests { get; set; } = new();
        public List<CampPharmacistRequest> RespondedRequests { get; set; } = new();
    }

    public class PatientHistoryViewModel
    {
        public MediCamp.Models.Domain.TriageRecord TriageRecord { get; set; } = new();
        public MediCamp.Models.Domain.Consultation? Consultation { get; set; }
        public List<MediCamp.Models.Domain.PrescriptionItem> PrescriptionItems { get; set; } = new();
    }

    public class HostManageInventoryViewModel
    {
        public MediCamp.Models.Domain.Camp Camp { get; set; } = new();
        public List<MediCamp.Models.Domain.CampInventory> CurrentInventory { get; set; } = new();
        public List<MediCamp.Models.Domain.MasterMedicine> AvailableMedicines { get; set; } = new();
        
        [Required(ErrorMessage = "Please select a medicine.")]
        public int SelectedMedicineId { get; set; }

        [Required]
        [Range(1, 100000, ErrorMessage = "Quantity must be greater than 0.")]
        public int QuantityToAllocate { get; set; }
    }

    public class HostMonitorCampViewModel
    {
        public MediCamp.Models.Domain.Camp Camp { get; set; } = new();
        
        [Required]
        [Range(0.01, 10000000, ErrorMessage = "Expense must be greater than 0.")]
        public decimal AdditionalExpense { get; set; }
        
        public string? ExpenseNotes { get; set; }
    }

    public class VolunteerDashboardViewModel
    {
        public List<MediCamp.Models.Domain.Camp> ApprovedCamps { get; set; } = new();
        public MediCamp.Models.Domain.Camp? ActiveCamp { get; set; }
        public List<ApplicationUser> SearchResults { get; set; } = new();
        public string? SearchQuery { get; set; }
    }

    public class TriageFormViewModel
    {
        [Required]
        public int CampId { get; set; }
        
        [Required]
        public string PatientId { get; set; } = string.Empty;
        
        public ApplicationUser? Patient { get; set; }
        
        [MaxLength(20)]
        public string? BloodPressure { get; set; }
        
        [Range(90, 110, ErrorMessage = "Invalid Temperature")]
        public double? TemperatureF { get; set; }
        
        [Range(2, 300, ErrorMessage = "Invalid Weight")]
        public double? WeightKg { get; set; }
        
        [Range(30, 250, ErrorMessage = "Invalid Height")]
        public double? HeightCm { get; set; }
        
        public double? BMI { get; set; }
        
        [MaxLength(500)]
        public string? PresentingSymptoms { get; set; }
        
        [Required]
        public string UrgencyLevel { get; set; } = "Normal";
    }

    public class VolunteerFollowUpViewModel
    {
        public MediCamp.Models.Domain.Camp? Camp { get; set; }
        public List<MediCamp.Models.Domain.PatientFollowUp> FollowUps { get; set; } = new();
    }

    // ==========================================
    // PHASE 6 — DOCTOR
    // ==========================================

    public class DoctorQueueViewModel
    {
        /// <summary>All camps the Doctor has been approved for that are currently Ongoing.</summary>
        public List<MediCamp.Models.Domain.Camp> ApprovedCamps { get; set; } = new();

        /// <summary>The camp the doctor selected for this session.</summary>
        public MediCamp.Models.Domain.Camp? ActiveCamp { get; set; }

        /// <summary>Triage records not yet seen by a doctor, sorted by urgency then token.</summary>
        public List<MediCamp.Models.Domain.TriageRecord> Queue { get; set; } = new();

        public int EmergencyCount => Queue.Count(t => t.UrgencyLevel == "Emergency");
        public int UrgentCount    => Queue.Count(t => t.UrgencyLevel == "Urgent");
        public int NormalCount    => Queue.Count(t => t.UrgencyLevel == "Normal");
    }

    public class ConsultationPrescriptionItemInput
    {
        public int MasterMedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;        // e.g. "1+0+1"
        public int DurationDays { get; set; }
        public string? Instructions { get; set; }
        public int QuantityPrescribed { get; set; }
    }

    public class PatientVisitHistoryItem
    {
        public MediCamp.Models.Domain.TriageRecord Triage { get; set; } = new();
        public MediCamp.Models.Domain.Consultation? Consultation { get; set; }
        public List<MediCamp.Models.Domain.PrescriptionItem> PrescriptionItems { get; set; } = new();
        public MediCamp.Models.Domain.Referral? Referral { get; set; }
        public string CampTitle { get; set; } = string.Empty;
    }

    public class ConsultationWorkspaceViewModel
    {
        // Current triage being consulted
        public MediCamp.Models.Domain.TriageRecord TriageRecord { get; set; } = new();
        public ApplicationUser? Patient { get; set; }

        // Patient's full longitudinal history (all past visits, excluding current)
        public List<PatientVisitHistoryItem> PastVisits { get; set; } = new();

        // Camp context for medicine selection
        public int CampId { get; set; }
        public List<MediCamp.Models.Domain.CampInventory> CampInventory { get; set; } = new();

        // Fields the doctor fills in
        [System.ComponentModel.DataAnnotations.MaxLength(250)]
        public string? Diagnosis { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? ClinicalNotes { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? Advice { get; set; }

        // Referral (optional)
        public bool AddReferral { get; set; } = false;
        public string? ReferredHospital { get; set; }
        public string? ReferralReason { get; set; }
        public string ReferralUrgency { get; set; } = "Routine";

        // Available hospitals for referral dropdown
        public List<MediCamp.Models.Domain.Hospital> AvailableHospitals { get; set; } = new();
    }

    // ==========================================
    // PHASE 6 — PHARMACIST
    // ==========================================

    public class PharmacistDashboardViewModel
    {
        public List<MediCamp.Models.Domain.Camp> ApprovedCamps { get; set; } = new();
        public MediCamp.Models.Domain.Camp? ActiveCamp { get; set; }
        public int PendingPrescriptionsCount { get; set; }
        public int DispensedTodayCount { get; set; }
        public int LowStockItemsCount { get; set; }
    }

    public class PrescriptionQueueViewModel
    {
        public MediCamp.Models.Domain.Camp Camp { get; set; } = new();
        public List<PrescriptionQueueItem> PendingPrescriptions { get; set; } = new();
        public List<PrescriptionQueueItem> DispensedPrescriptions { get; set; } = new();
    }

    public class PrescriptionQueueItem
    {
        public int PrescriptionId { get; set; }
        public int ConsultationId { get; set; }
        public int TokenNumber { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string? Diagnosis { get; set; }
        public int ItemCount { get; set; }
        public DateTime ConsultedAt { get; set; }
        public bool IsDispensed { get; set; }
        public DateTime? DispensedAt { get; set; }
    }

    public class DispensingItemInput
    {
        public int PrescriptionItemId { get; set; }
        public int MasterMedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? Instructions { get; set; }
        public int QuantityPrescribed { get; set; }
        public int AvailableStock { get; set; }
        public int QuantityToDispense { get; set; }
        public int? SubstituteMedicineId { get; set; }
        public bool IsDispensed { get; set; }
    }

    public class DispensingViewModel
    {
        public MediCamp.Models.Domain.Prescription Prescription { get; set; } = new();
        public MediCamp.Models.Domain.Consultation? Consultation { get; set; }
        public MediCamp.Models.Domain.TriageRecord? TriageRecord { get; set; }
        public ApplicationUser? Patient { get; set; }
        public ApplicationUser? Doctor { get; set; }
        public int CampId { get; set; }
        public List<DispensingItemInput> Items { get; set; } = new();
        public List<MediCamp.Models.Domain.CampInventory> AvailableCampInventory { get; set; } = new();
    }

    public class PharmacistInventoryItem
    {
        public int InventoryId { get; set; }
        public int MasterMedicineId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public string DosageForm { get; set; } = string.Empty;
        public string Strength { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Allocated { get; set; }
        public int Dispensed { get; set; }
        public int Remaining => Allocated - Dispensed;
        public double StockPercentage => Allocated > 0 ? (double)Remaining / Allocated * 100 : 0;
        public bool IsLowStock => Remaining > 0 && StockPercentage < 15;
        public bool IsOutOfStock => Remaining <= 0;
    }

    public class PharmacistInventoryViewModel
    {
        public MediCamp.Models.Domain.Camp Camp { get; set; } = new();
        public List<PharmacistInventoryItem> Inventory { get; set; } = new();
        public int LowStockCount => Inventory.Count(i => i.IsLowStock);
        public int OutOfStockCount => Inventory.Count(i => i.IsOutOfStock);
        public int TotalItems => Inventory.Count;

        // For restock form
        public int RestockMedicineId { get; set; }
        public int RestockQuantity { get; set; }
    }

    // ==========================================
    // PHASE 7 — PATIENT PORTAL & BLOOD DONATION
    // ==========================================

    public class PatientVisitTimelineItem
    {
        public int TriageId { get; set; }
        public string CampTitle { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public int TokenNumber { get; set; }
        public string UrgencyLevel { get; set; } = "Normal";

        // Vitals
        public string? BloodPressure { get; set; }
        public double? TemperatureF { get; set; }
        public double? WeightKg { get; set; }
        public double? HeightCm { get; set; }
        public double? BMI { get; set; }
        public string? PresentingSymptoms { get; set; }

        // Doctor Consultation
        public string? DoctorName { get; set; }
        public string? Diagnosis { get; set; }
        public string? ClinicalNotes { get; set; }
        public string? Advice { get; set; }
        public DateTime? ConsultedAt { get; set; }

        // Prescription
        public int? PrescriptionId { get; set; }
        public bool IsPrescriptionDispensed { get; set; }
        public DateTime? DispensedAt { get; set; }
        public List<PrescriptionItemDetail> PrescriptionItems { get; set; } = new();

        // Referral
        public string? ReferredHospital { get; set; }
        public string? ReferralReason { get; set; }
        public string? ReferralUrgency { get; set; }

        // Follow-Up
        public string? FollowUpReason { get; set; }
        public DateTime? FollowUpScheduledDate { get; set; }
        public string? FollowUpStatus { get; set; }
    }

    public class PrescriptionItemDetail
    {
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public string? Instructions { get; set; }
        public int QuantityPrescribed { get; set; }
        public int QuantityDispensed { get; set; }
    }

    public class PatientProfileHistoryViewModel
    {
        public ApplicationUser Patient { get; set; } = new();
        public MediCamp.Models.Domain.BloodDonationProfile? BloodProfile { get; set; }
        public List<PatientVisitTimelineItem> Visits { get; set; } = new();
        public int TotalVisitsCount => Visits.Count;
        public int TotalPrescriptionsCount => Visits.Count(v => v.PrescriptionId.HasValue);
    }

    public class DigitalPrescriptionViewModel
    {
        public MediCamp.Models.Domain.Prescription Prescription { get; set; } = new();
        public MediCamp.Models.Domain.Consultation? Consultation { get; set; }
        public MediCamp.Models.Domain.TriageRecord? TriageRecord { get; set; }
        public ApplicationUser? Patient { get; set; }
        public ApplicationUser? Doctor { get; set; }
        public MediCamp.Models.Domain.Camp? Camp { get; set; }
        public List<MediCamp.Models.Domain.PrescriptionItem> Items { get; set; } = new();
        public MediCamp.Models.Domain.Referral? Referral { get; set; }
    }

    public class DonorSearchResultItem
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Upazila { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsAvailableDonor { get; set; }
        public DateTime? LastDonatedDate { get; set; }
        public int TotalDonationsCount { get; set; }
        public bool IsEligibleToDonate
        {
            get
            {
                if (!IsAvailableDonor) return false;
                if (!LastDonatedDate.HasValue) return true;
                return (DateTime.UtcNow - LastDonatedDate.Value).TotalDays >= 90;
            }
        }
    }

    public class BloodDonationHubViewModel
    {
        public ApplicationUser? CurrentUser { get; set; }
        public MediCamp.Models.Domain.BloodDonationProfile? MyProfile { get; set; }

        // Search parameters & results
        public string? SelectedBloodGroup { get; set; }
        public string? SelectedDistrict { get; set; }
        public string? SelectedUpazila { get; set; }
        public List<DonorSearchResultItem> Donors { get; set; } = new();

        // Requests feed
        public List<MediCamp.Models.Domain.BloodRequest> OpenRequests { get; set; } = new();
        public List<MediCamp.Models.Domain.BloodRequest> MyRequests { get; set; } = new();
        public List<MediCamp.Models.Domain.BloodDonationLog> MyDonationLogs { get; set; } = new();

        // Reference lists
        public List<string> AllBloodGroups { get; set; } = new() { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
        public List<string> Districts { get; set; } = new();
        public List<string> Upazilas { get; set; } = new();
    }

    public class CreateBloodRequestInput
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string BloodGroup { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.Range(1, 20)]
        public int UnitsRequired { get; set; } = 1;

        [System.ComponentModel.DataAnnotations.Required]
        public string Urgency { get; set; } = "Urgent"; // Normal, Urgent, Critical

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(150)]
        public string HospitalName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string District { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string? Upazila { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(20)]
        public string ContactNumber { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? Reason { get; set; }

        public DateTime? NeededByDate { get; set; }
    }

    public class LogBloodDonationInput
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(150)]
        public string VenueOrHospital { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        public DateTime DonatedDate { get; set; } = DateTime.UtcNow;

        public int? BloodRequestId { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class VolunteerScheduleFollowUpInput
    {
        public int CampId { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; } = DateTime.UtcNow.AddDays(3);
    }
}
