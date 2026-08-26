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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
