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

    public class CampInventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CampId { get; set; }
        [ForeignKey("CampId")]
        public Camp? Camp { get; set; }

        [Required]
        public int MasterMedicineId { get; set; }
        [ForeignKey("MasterMedicineId")]
        public MasterMedicine? MasterMedicine { get; set; }

        public int QuantityAllocated { get; set; }
        public int QuantityDispensed { get; set; } = 0;
    }

    public class TriageRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CampId { get; set; }
        [ForeignKey("CampId")]
        public Camp? Camp { get; set; }

        [Required]
        public string PatientId { get; set; } = string.Empty;
        [ForeignKey("PatientId")]
        public ApplicationUser? Patient { get; set; }

        [Required]
        public string VolunteerId { get; set; } = string.Empty;
        [ForeignKey("VolunteerId")]
        public ApplicationUser? Volunteer { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(20)]
        public string? BloodPressure { get; set; }
        
        public double? TemperatureF { get; set; }
        public double? WeightKg { get; set; }
        public double? HeightCm { get; set; }
        public double? BMI { get; set; }
        
        [MaxLength(500)]
        public string? PresentingSymptoms { get; set; }
        
        [MaxLength(20)]
        public string UrgencyLevel { get; set; } = "Normal"; // Normal, Urgent, Emergency
        
        public int TokenNumber { get; set; }
        
        public bool IsSeenByDoctor { get; set; } = false;
    }

    public class Consultation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TriageRecordId { get; set; }
        [ForeignKey("TriageRecordId")]
        public TriageRecord? TriageRecord { get; set; }

        [Required]
        public string DoctorId { get; set; } = string.Empty;
        [ForeignKey("DoctorId")]
        public ApplicationUser? Doctor { get; set; }

        public DateTime ConsultedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? ClinicalNotes { get; set; }

        [MaxLength(250)]
        public string? Diagnosis { get; set; }

        [MaxLength(500)]
        public string? Advice { get; set; }
    }

    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConsultationId { get; set; }
        [ForeignKey("ConsultationId")]
        public Consultation? Consultation { get; set; }

        public bool IsDispensed { get; set; } = false;
        
        public string? DispensedByPharmacistId { get; set; }
        [ForeignKey("DispensedByPharmacistId")]
        public ApplicationUser? Pharmacist { get; set; }
        
        public DateTime? DispensedAt { get; set; }
    }

    public class PrescriptionItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PrescriptionId { get; set; }
        [ForeignKey("PrescriptionId")]
        public Prescription? Prescription { get; set; }

        [Required]
        public int MasterMedicineId { get; set; }
        [ForeignKey("MasterMedicineId")]
        public MasterMedicine? MasterMedicine { get; set; }

        [MaxLength(50)]
        public string Dosage { get; set; } = string.Empty; // e.g. "1+0+1"

        public int DurationDays { get; set; }
        
        [MaxLength(150)]
        public string? Instructions { get; set; } // e.g. "After meal"
        
        public int QuantityPrescribed { get; set; }
        public int QuantityDispensed { get; set; } = 0;
    }

    public class Referral
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConsultationId { get; set; }
        [ForeignKey("ConsultationId")]
        public Consultation? Consultation { get; set; }

        [Required]
        [MaxLength(150)]
        public string ReferredHospital { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Reason { get; set; }
        
        [MaxLength(20)]
        public string Urgency { get; set; } = "Routine"; // Routine, Urgent
    }
}
