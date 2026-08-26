using MediCamp.Models;
using MediCamp.Models.Domain;

namespace MediCamp.Data
{
    public static class DbSeeder
    {
        public static void SeedData(ApplicationDbContext dbContext)
        {
            // 1. Seed Users
            if (!dbContext.Users.Any())
            {
                var users = new List<ApplicationUser>
                {
                    new ApplicationUser
                    {
                        Id = "usr-admin-01",
                        FullName = "Prof. Dr. Tanvir Hasan",
                        Email = "admin@medicamp.org",
                        PhoneNumber = "01711000001",
                        NID = "19782690011223344",
                        Role = SystemRoles.Admin,
                        District = "Dhaka",
                        Upazila = "Dhanmondi",
                        Address = "MediCamp Central HQ, Level 4, Dhanmondi 27, Dhaka",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-6),
                        LastLoginAt = DateTime.UtcNow.AddHours(-1),
                        PasswordHash = "Admin@123"
                    },
                    new ApplicationUser
                    {
                        Id = "usr-host-01",
                        FullName = "Shahidul Alam",
                        Email = "host@brachospital.org",
                        PhoneNumber = "01819000002",
                        NID = "19822690022334455",
                        Role = SystemRoles.Host,
                        District = "Dhaka",
                        Upazila = "Mohakhali",
                        Address = "BRAC Centre, 75 Mohakhali, Dhaka-1212",
                        OrganizationName = "BRAC Community Health Initiative",
                        OrganizationRegNo = "NGOAB-2018-0492",
                        FocalPersonContact = "01819000002",
                        HostApprovalStatus = "Approved",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-4),
                        LastLoginAt = DateTime.UtcNow.AddHours(-3),
                        PasswordHash = "Host@123"
                    },
                    new ApplicationUser
                    {
                        Id = "usr-host-02",
                        FullName = "Dr. Shamsul Huda",
                        Email = "host.green@gmail.com",
                        PhoneNumber = "01912000003",
                        NID = "19862690033445566",
                        Role = SystemRoles.Host,
                        District = "Sylhet",
                        Upazila = "Kotwali",
                        Address = "Green Crescent Foundation, Zindabazar, Sylhet",
                        OrganizationName = "Green Crescent Humanitarian Aid",
                        OrganizationType = "NGO",
                        OrganizationRegNo = "NGOAB-2024-1180",
                        FocalPersonContact = "01912000003",
                        HostApprovalStatus = "Pending",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        PasswordHash = "Host@123"
                    },
                    new ApplicationUser
                    {
                        Id = "usr-host-03",
                        FullName = "Mahmudur Rahman",
                        Email = "contact@carebd.org",
                        PhoneNumber = "01711000099",
                        NID = "19852690099887766",
                        Role = SystemRoles.Host,
                        District = "Chittagong",
                        Upazila = "Agrabad",
                        Address = "Agrabad Commercial Area, Chittagong",
                        OrganizationName = "Care BD Relief Trust",
                        OrganizationType = "Community Group",
                        OrganizationRegNo = "COMM-2023-88",
                        FocalPersonContact = "01711000099",
                        HostApprovalStatus = "Rejected",
                        HostRejectionReason = "Organization registration documents could not be verified.",
                        IsActive = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        PasswordHash = "Host@123"
                    },
                    new ApplicationUser
                    {
                        Id = "usr-doc-01",
                        FullName = "Dr. Rafiqul Rahman",
                        Email = "doctor.rahman@medicamp.org",
                        PhoneNumber = "01722000004",
                        NID = "19882690044556677",
                        Role = SystemRoles.Doctor,
                        District = "Dhaka",
                        Upazila = "Shahbagh",
                        Address = "BSMMU Doctors Quarter, Shahbagh, Dhaka",
                        MedicalSpecialization = "MBBS, FCPS (Internal Medicine)",
                        BMDCRegNo = "BMDC-A-48291",
                        BloodGroup = "B+",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-3),
                        LastLoginAt = DateTime.UtcNow.AddHours(-5),
                        PasswordHash = "Doctor@123"
                    },
                    new ApplicationUser
                    {
                        Id = "usr-doc-02",
                        FullName = "Dr. Fatima Tuz Zohra",
                        Email = "doctor.fatima@medicamp.org",
                        PhoneNumber = "01733000005",
                        NID = "19902690055667788",
                        Role = SystemRoles.Doctor,
                        District = "Chittagong",
                        Upazila = "Panchlaish",
                        Address = "Chittagong Medical College Hospital Area",
                        MedicalSpecialization = "MBBS, DGO (Obstetrics & Gynecology)",
                        BMDCRegNo = "BMDC-A-59124",
                        BloodGroup = "O+",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-2),
                        PasswordHash = "Doctor@123"
                    },
                    new ApplicationUser
                    {
                        Id = "usr-vol-01",
                        FullName = "Karim Ahmed",
                        Email = "volunteer.karim@medicamp.org",
                        PhoneNumber = "01511000006",
                        NID = "19962690066778899",
                        Role = SystemRoles.Volunteer,
                        District = "Kurigram",
                        Upazila = "Chilmari",
                        Address = "Char Chilmari, Ward 3, Kurigram",
                        BloodGroup = "A+",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-2),
                        LastLoginAt = DateTime.UtcNow.AddHours(-2),
                        PasswordHash = "Volunteer@123"
                    },
                    new ApplicationUser
                    {
                        Id = "usr-vol-02",
                        FullName = "Sadia Sultana",
                        Email = "volunteer.sadia@medicamp.org",
                        PhoneNumber = "01611000007",
                        NID = "19982690077889900",
                        Role = SystemRoles.Volunteer,
                        District = "Sunamganj",
                        Upazila = "Tahirpur",
                        Address = "Tahirpur Bazar, Sunamganj",
                        BloodGroup = "AB+",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-20),
                        PasswordHash = "Volunteer@123"
                    },
                    new ApplicationUser
                    {
                        Id = "usr-pharma-01",
                        FullName = "Nusrat Poly",
                        Email = "pharma.poly@medicamp.org",
                        PhoneNumber = "01755000008",
                        NID = "19942690088990011",
                        Role = SystemRoles.Pharmacist,
                        District = "Rangpur",
                        Upazila = "Kotwali",
                        Address = "Dhap Medical Road, Rangpur",
                        BloodGroup = "O-",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-2),
                        LastLoginAt = DateTime.UtcNow.AddDays(-1),
                        PasswordHash = "Pharma@123"
                    },
                    new ApplicationUser
                    {
                        Id = "usr-pat-01",
                        FullName = "Anwar Hossain",
                        Email = "patient.anwar@medicamp.org",
                        PhoneNumber = "01822000009",
                        NID = "19852691234567890",
                        Role = SystemRoles.Patient,
                        District = "Kurigram",
                        Upazila = "Chilmari",
                        Address = "South Ramna Village, Chilmari, Kurigram",
                        BloodGroup = "B+",
                        DateOfBirth = new DateTime(1985, 4, 12, 0, 0, 0, DateTimeKind.Utc),
                        Gender = "Male",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-1),
                        PasswordHash = "Patient@123"
                    },
                    new ApplicationUser
                    {
                        Id = "usr-pat-02",
                        FullName = "Mariam Begum",
                        Email = "patient.mariam@medicamp.org",
                        PhoneNumber = "01933000010",
                        NID = "19922695432109876",
                        Role = SystemRoles.Patient,
                        District = "Sunamganj",
                        Upazila = "Tahirpur",
                        Address = "Bishwamvarpur Road, Sunamganj",
                        BloodGroup = "A+",
                        DateOfBirth = new DateTime(1992, 9, 25, 0, 0, 0, DateTimeKind.Utc),
                        Gender = "Female",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-15),
                        PasswordHash = "Patient@123"
                    }
                };

                dbContext.Users.AddRange(users);
                dbContext.SaveChanges();
            }

            // 2. Seed Camps
            if (!dbContext.Camps.Any())
            {
                var camps = new List<Camp>
                {
                    new Camp
                    {
                        Title = "Kurigram Chilmari Char Free Healthcare & Screening Camp",
                        CampType = "General Healthcare & Triage",
                        District = "Kurigram",
                        Upazila = "Chilmari",
                        Venue = "Chilmari High School Ground",
                        StartDate = DateTime.UtcNow.AddDays(3),
                        EndDate = DateTime.UtcNow.AddDays(5),
                        ExpectedPatients = 1200,
                        RegisteredPatientsCount = 0,
                        ServedPatientsCount = 0,
                        TotalBudget = 150000.00m,
                        UtilizedBudget = 45000.00m,
                        Status = "Scheduled",
                        HostId = "usr-host-01",
                        Description = "Free medical care and essential diagnostic screening for flood-affected char communities."
                    },
                    new Camp
                    {
                        Title = "Sunamganj Haor Remote Maternal & Child Healthcare Camp",
                        CampType = "Maternal & Child Care",
                        District = "Sunamganj",
                        Upazila = "Tahirpur",
                        Venue = "Tahirpur Union Parishad Health Complex",
                        StartDate = DateTime.UtcNow.AddDays(-1),
                        EndDate = DateTime.UtcNow.AddDays(2),
                        ExpectedPatients = 850,
                        RegisteredPatientsCount = 450,
                        ServedPatientsCount = 412,
                        TotalBudget = 120000.00m,
                        UtilizedBudget = 88000.00m,
                        Status = "Ongoing",
                        HostId = "usr-host-01",
                        Description = "Specialized obstetric, gynecological, and pediatric healthcare services in remote haor areas."
                    },
                    new Camp
                    {
                        Title = "Bandarban Ruma Hill Tracts Community Vision & Dental Camp",
                        CampType = "Eye & Dental Specialist",
                        District = "Bandarban",
                        Upazila = "Ruma",
                        Venue = "Ruma Model Primary School",
                        StartDate = DateTime.UtcNow.AddDays(12),
                        EndDate = DateTime.UtcNow.AddDays(14),
                        ExpectedPatients = 600,
                        RegisteredPatientsCount = 0,
                        ServedPatientsCount = 0,
                        TotalBudget = 95000.00m,
                        UtilizedBudget = 15000.00m,
                        Status = "Scheduled",
                        HostId = "usr-host-02",
                        Description = "Dental procedures, eye examinations, and free distribution of prescription glasses."
                    }
                };

                dbContext.Camps.AddRange(camps);
                dbContext.SaveChanges();
            }

            // 3. Seed Master Medicines
            if (!dbContext.MasterMedicines.Any())
            {
                var medicines = new List<MasterMedicine>
                {
                    new MasterMedicine { BrandName = "Napa Extra", GenericName = "Paracetamol + Caffeine", DosageForm = "Tablet", Strength = "500mg+65mg", Category = "Analgesic", IsEssential = true },
                    new MasterMedicine { BrandName = "Seclo 20", GenericName = "Omeprazole", DosageForm = "Capsule", Strength = "20mg", Category = "Gastric", IsEssential = true },
                    new MasterMedicine { BrandName = "Cef-3", GenericName = "Cefixime", DosageForm = "Capsule", Strength = "200mg", Category = "Antibiotic", IsEssential = true },
                    new MasterMedicine { BrandName = "Histacin", GenericName = "Chlorpheniramine", DosageForm = "Tablet", Strength = "4mg", Category = "Antihistamine", IsEssential = true },
                    new MasterMedicine { BrandName = "Bexidal", GenericName = "Multivitamin", DosageForm = "Syrup", Strength = "100ml", Category = "Vitamin", IsEssential = true }
                };

                dbContext.MasterMedicines.AddRange(medicines);
                dbContext.SaveChanges();
            }

            // 4. Seed Locations (Divisions, Districts, Upazilas for dynamic lookup)
            if (!dbContext.Locations.Any())
            {
                var locations = new List<Location>
                {
                    // Dhaka Division
                    new Location { Division = "Dhaka", District = "Dhaka", Upazila = "Dhanmondi" },
                    new Location { Division = "Dhaka", District = "Dhaka", Upazila = "Mohakhali" },
                    new Location { Division = "Dhaka", District = "Dhaka", Upazila = "Gulshan" },
                    new Location { Division = "Dhaka", District = "Dhaka", Upazila = "Uttara" },
                    new Location { Division = "Dhaka", District = "Dhaka", Upazila = "Mirpur" },
                    new Location { Division = "Dhaka", District = "Dhaka", Upazila = "Savar" },
                    new Location { Division = "Dhaka", District = "Gazipur", Upazila = "Gazipur Sadar" },
                    new Location { Division = "Dhaka", District = "Gazipur", Upazila = "Sreepur" },
                    new Location { Division = "Dhaka", District = "Gazipur", Upazila = "Kaliakair" },
                    new Location { Division = "Dhaka", District = "Narayanganj", Upazila = "Narayanganj Sadar" },
                    new Location { Division = "Dhaka", District = "Narayanganj", Upazila = "Rupganj" },
                    new Location { Division = "Dhaka", District = "Narayanganj", Upazila = "Sonargaon" },

                    // Rangpur Division
                    new Location { Division = "Rangpur", District = "Kurigram", Upazila = "Chilmari" },
                    new Location { Division = "Rangpur", District = "Kurigram", Upazila = "Nageswari" },
                    new Location { Division = "Rangpur", District = "Kurigram", Upazila = "Bhurungamari" },
                    new Location { Division = "Rangpur", District = "Kurigram", Upazila = "Kurigram Sadar" },
                    new Location { Division = "Rangpur", District = "Kurigram", Upazila = "Ulipur" },
                    new Location { Division = "Rangpur", District = "Rangpur", Upazila = "Kotwali" },
                    new Location { Division = "Rangpur", District = "Rangpur", Upazila = "Mithapukur" },
                    new Location { Division = "Rangpur", District = "Rangpur", Upazila = "Pirganj" },

                    // Sylhet Division
                    new Location { Division = "Sylhet", District = "Sunamganj", Upazila = "Tahirpur" },
                    new Location { Division = "Sylhet", District = "Sunamganj", Upazila = "Bishwamvarpur" },
                    new Location { Division = "Sylhet", District = "Sunamganj", Upazila = "Chhatak" },
                    new Location { Division = "Sylhet", District = "Sunamganj", Upazila = "Sunamganj Sadar" },
                    new Location { Division = "Sylhet", District = "Sylhet", Upazila = "Kotwali" },
                    new Location { Division = "Sylhet", District = "Sylhet", Upazila = "Beanibazar" },
                    new Location { Division = "Sylhet", District = "Sylhet", Upazila = "Gowainghat" },

                    // Chittagong Division
                    new Location { Division = "Chittagong", District = "Bandarban", Upazila = "Ruma" },
                    new Location { Division = "Chittagong", District = "Bandarban", Upazila = "Thanchi" },
                    new Location { Division = "Chittagong", District = "Bandarban", Upazila = "Lama" },
                    new Location { Division = "Chittagong", District = "Bandarban", Upazila = "Bandarban Sadar" },
                    new Location { Division = "Chittagong", District = "Chittagong", Upazila = "Panchlaish" },
                    new Location { Division = "Chittagong", District = "Chittagong", Upazila = "Agrabad" },
                    new Location { Division = "Chittagong", District = "Chittagong", Upazila = "Hathazari" },
                    new Location { Division = "Chittagong", District = "Chittagong", Upazila = "Sitakunda" },

                    // Khulna Division
                    new Location { Division = "Khulna", District = "Khulna", Upazila = "Khulna Sadar" },
                    new Location { Division = "Khulna", District = "Khulna", Upazila = "Rupsha" },
                    new Location { Division = "Khulna", District = "Khulna", Upazila = "Batiaghata" },

                    // Barisal Division
                    new Location { Division = "Barisal", District = "Barisal", Upazila = "Barisal Sadar" },
                    new Location { Division = "Barisal", District = "Barisal", Upazila = "Bakerganj" },

                    // Rajshahi Division
                    new Location { Division = "Rajshahi", District = "Rajshahi", Upazila = "Boalia" },
                    new Location { Division = "Rajshahi", District = "Rajshahi", Upazila = "Paba" },

                    // Mymensingh Division
                    new Location { Division = "Mymensingh", District = "Mymensingh", Upazila = "Mymensingh Sadar" },
                    new Location { Division = "Mymensingh", District = "Mymensingh", Upazila = "Muktagacha" }
                };

                dbContext.Locations.AddRange(locations);
                dbContext.SaveChanges();
            }
        }
    }
}
