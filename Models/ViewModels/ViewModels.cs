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
}
