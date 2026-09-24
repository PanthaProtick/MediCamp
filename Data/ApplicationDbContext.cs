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
        public DbSet<CampStaffRequest> CampStaffRequests { get; set; }
        public DbSet<CampVolunteerRequest> CampVolunteerRequests { get; set; }
        public DbSet<CampPharmacistRequest> CampPharmacistRequests { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<BloodGroupMaster> BloodGroupMasters { get; set; }
        public DbSet<PatientFollowUp> PatientFollowUps { get; set; }
        public DbSet<CampPatientRegistration> CampPatientRegistrations { get; set; }
        public DbSet<BloodRequest> BloodRequests { get; set; }
        public DbSet<BloodDonationLog> BloodDonationLogs { get; set; }
        public DbSet<CampExpense> CampExpenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                v => !v.HasValue ? v : (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)),
                v => !v.HasValue ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
        }
    }
}
