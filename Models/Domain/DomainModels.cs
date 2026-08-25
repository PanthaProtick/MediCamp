using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediCamp.Models.Domain
{
    public class Camp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CampType { get; set; } = "General Healthcare"; // General, Eye Camp, Maternal Care, Dental, Blood Drive

        [Required]
        [MaxLength(50)]
        public string District { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Upazila { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Venue { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int ExpectedPatients { get; set; }

        public int RegisteredPatientsCount { get; set; }

        public int ServedPatientsCount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBudget { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UtilizedBudget { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Ongoing, Completed, Cancelled

        public string? HostId { get; set; }

        [ForeignKey("HostId")]
        public ApplicationUser? Host { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Division { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string District { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Upazila { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Union { get; set; }

        [MaxLength(50)]
        public string? Village { get; set; }
    }

    public class MasterMedicine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string BrandName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string GenericName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string DosageForm { get; set; } = "Tablet"; // Tablet, Capsule, Syrup, Suspension, Ointment, Injection

        [MaxLength(50)]
        public string Strength { get; set; } = string.Empty; // 500mg, 10mg, 100ml

        [MaxLength(50)]
        public string Category { get; set; } = "Analgesic"; // Antibiotic, Antihistamine, Antidiabetic, Analgesic, Vitamin

        public bool IsEssential { get; set; } = true;
    }

    public class BloodDonationProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public bool IsAvailableDonor { get; set; } = true;

        [MaxLength(10)]
        public string BloodGroup { get; set; } = string.Empty;

        [MaxLength(50)]
        public string District { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Upazila { get; set; } = string.Empty;

        public DateTime? LastDonatedDate { get; set; }

        public int TotalDonationsCount { get; set; } = 0;

        public double? WeightKg { get; set; }
    }
}
