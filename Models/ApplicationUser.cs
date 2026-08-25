using System.ComponentModel.DataAnnotations;

namespace MediCamp.Models
{
    public class ApplicationUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(20)]
        [Display(Name = "National ID (NID)")]
        public string? NID { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(15)]
        public string? Gender { get; set; } = "Male"; // Male, Female, Other

        [MaxLength(10)]
        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; } // A+, A-, B+, B-, AB+, AB-, O+, O-

        [MaxLength(50)]
        public string? District { get; set; }

        [MaxLength(50)]
        public string? Upazila { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        public string Role { get; set; } = SystemRoles.Patient;

        // Fields specific to Host (NGO)
        [MaxLength(150)]
        [Display(Name = "Organization / NGO Name")]
        public string? OrganizationName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Registration / NGOAB License No")]
        public string? OrganizationRegNo { get; set; }

        [MaxLength(50)]
        [Display(Name = "Focal Person Contact")]
        public string? FocalPersonContact { get; set; }

        [MaxLength(20)]
        public string HostApprovalStatus { get; set; } = "Approved"; // Approved, Pending, Rejected

        public string? HostRejectionReason { get; set; }

        // Fields specific to Doctor
        [MaxLength(100)]
        [Display(Name = "Medical Specialization / Degrees")]
        public string? MedicalSpecialization { get; set; }

        [MaxLength(50)]
        [Display(Name = "BMDC Registration Number")]
        public string? BMDCRegNo { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public string PasswordHash { get; set; } = "Pass@123"; // In-memory mock password
    }

    public static class SystemRoles
    {
        public const string Admin = "Admin";
        public const string Host = "Host";
        public const string Doctor = "Doctor";
        public const string Volunteer = "Volunteer";
        public const string Pharmacist = "Pharmacist";
        public const string Patient = "Patient";

        public static readonly string[] AllRoles = [Admin, Host, Doctor, Volunteer, Pharmacist, Patient];
    }
}
