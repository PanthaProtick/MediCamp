using System.ComponentModel.DataAnnotations;

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

        // Host NGO Specific
        [Display(Name = "Organization / NGO Legal Name")]
        public string? OrganizationName { get; set; }

        [Display(Name = "Registration / NGOAB License No")]
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
}
