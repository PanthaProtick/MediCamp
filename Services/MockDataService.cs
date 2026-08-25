using MediCamp.Models;
using MediCamp.Models.ViewModels;

namespace MediCamp.Services
{
    public class MockDataService : IMockDataService
    {
        private readonly List<ApplicationUser> _users = new();
        private readonly List<CampOverviewItem> _camps = new();
        private readonly object _lock = new();

        public MockDataService()
        {
            SeedInitialData();
        }

        private void SeedInitialData()
        {
            lock (_lock)
            {
                if (_users.Any()) return;

                // 1. Admin
                _users.Add(new ApplicationUser
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
                });

                // 2. Host (Approved NGO - BRAC)
                _users.Add(new ApplicationUser
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
                });

                // 3. Host (Pending NGO)
                _users.Add(new ApplicationUser
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
                    OrganizationRegNo = "NGOAB-2024-1180",
                    FocalPersonContact = "01912000003",
                    HostApprovalStatus = "Pending",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    PasswordHash = "Host@123"
                });

                // 4. Doctor (Rafiqul Rahman)
                _users.Add(new ApplicationUser
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
                });

                // 5. Doctor (Fatima)
                _users.Add(new ApplicationUser
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
                });

                // 6. Volunteer (Karim Ahmed)
                _users.Add(new ApplicationUser
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
                });

                // 7. Volunteer (Sadia)
                _users.Add(new ApplicationUser
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
                });

                // 8. Pharmacist (Nusrat Poly)
                _users.Add(new ApplicationUser
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
                });

                // 9. Patient (Anwar Hossain)
                _users.Add(new ApplicationUser
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
                    DateOfBirth = new DateTime(1985, 4, 12),
                    Gender = "Male",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1),
                    PasswordHash = "Patient@123"
                });

                // 10. Patient (Mariam Begum)
                _users.Add(new ApplicationUser
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
                    DateOfBirth = new DateTime(1992, 9, 25),
                    Gender = "Female",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    PasswordHash = "Patient@123"
                });

                // Seed Camps
                _camps.Add(new CampOverviewItem
                {
                    Id = 1,
                    Title = "Kurigram Chilmari Char Free Healthcare & Screening Camp",
                    CampType = "General Healthcare & Triage",
                    District = "Kurigram",
                    Upazila = "Chilmari",
                    Venue = "Chilmari High School Ground",
                    HostOrganization = "BRAC Community Health Initiative",
                    StartDate = DateTime.UtcNow.AddDays(3),
                    EndDate = DateTime.UtcNow.AddDays(5),
                    ExpectedPatients = 1200,
                    ServedPatientsCount = 0,
                    Status = "Scheduled"
                });

                _camps.Add(new CampOverviewItem
                {
                    Id = 2,
                    Title = "Sunamganj Haor Remote Maternal & Child Healthcare Camp",
                    CampType = "Maternal & Child Care",
                    District = "Sunamganj",
                    Upazila = "Tahirpur",
                    Venue = "Tahirpur Union Parishad Health Complex",
                    HostOrganization = "BRAC Community Health Initiative",
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    ExpectedPatients = 850,
                    ServedPatientsCount = 412,
                    Status = "Ongoing"
                });

                _camps.Add(new CampOverviewItem
                {
                    Id = 3,
                    Title = "Bandarban Ruma Hill Tracts Community Vision & Dental Camp",
                    CampType = "Eye & Dental Specialist",
                    District = "Bandarban",
                    Upazila = "Ruma",
                    Venue = "Ruma Model Primary School",
                    HostOrganization = "Green Crescent Humanitarian Aid",
                    StartDate = DateTime.UtcNow.AddDays(12),
                    EndDate = DateTime.UtcNow.AddDays(14),
                    ExpectedPatients = 600,
                    ServedPatientsCount = 0,
                    Status = "Scheduled"
                });

                _camps.Add(new CampOverviewItem
                {
                    Id = 4,
                    Title = "Satkhira Coastal Free Dispensary & Diabetes Screening",
                    CampType = "Non-Communicable Diseases",
                    District = "Satkhira",
                    Upazila = "Shyamnagar",
                    Venue = "Shyamnagar Cyclone Shelter & Community Center",
                    HostOrganization = "BRAC Community Health Initiative",
                    StartDate = DateTime.UtcNow.AddDays(-15),
                    EndDate = DateTime.UtcNow.AddDays(-13),
                    ExpectedPatients = 1400,
                    ServedPatientsCount = 1450,
                    Status = "Completed"
                });
            }
        }

        public List<ApplicationUser> GetAllUsers()
        {
            lock (_lock)
            {
                return _users.OrderByDescending(u => u.CreatedAt).ToList();
            }
        }

        public List<ApplicationUser> GetFilteredUsers(string? searchTerm, string? role, string? status)
        {
            lock (_lock)
            {
                var query = _users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLowerInvariant();
                    query = query.Where(u =>
                        u.FullName.ToLowerInvariant().Contains(term) ||
                        u.Email.ToLowerInvariant().Contains(term) ||
                        (u.NID != null && u.NID.Contains(term)) ||
                        u.PhoneNumber.Contains(term) ||
                        (u.District != null && u.District.ToLowerInvariant().Contains(term)) ||
                        (u.OrganizationName != null && u.OrganizationName.ToLowerInvariant().Contains(term))
                    );
                }

                if (!string.IsNullOrWhiteSpace(role) && role != "All")
                {
                    query = query.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(status) && status != "All")
                {
                    if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(u => u.IsActive);
                    }
                    else if (status.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(u => !u.IsActive);
                    }
                    else if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(u => u.HostApprovalStatus == "Pending");
                    }
                }

                return query.OrderByDescending(u => u.CreatedAt).ToList();
            }
        }

        public ApplicationUser? GetUserById(string id)
        {
            lock (_lock)
            {
                return _users.FirstOrDefault(u => u.Id == id);
            }
        }

        public ApplicationUser? GetUserByEmailOrNid(string identifier)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(identifier)) return null;
                var trimmed = identifier.Trim().ToLowerInvariant();

                return _users.FirstOrDefault(u =>
                    u.Email.ToLowerInvariant() == trimmed ||
                    (u.NID != null && u.NID.Trim() == identifier.Trim()) ||
                    u.PhoneNumber == identifier.Trim()
                );
            }
        }

        public (bool Success, string Message, ApplicationUser? User) Authenticate(string identifier, string password)
        {
            lock (_lock)
            {
                var user = GetUserByEmailOrNid(identifier);
                if (user == null)
                {
                    return (false, "No account found matching this Email or National ID (NID).", null);
                }

                if (!user.IsActive)
                {
                    return (false, "This account has been disabled by a system administrator.", null);
                }

                if (user.Role == SystemRoles.Host && user.HostApprovalStatus == "Pending")
                {
                    return (false, "Your NGO Host registration is currently under review by System Admins. You will be notified upon approval.", null);
                }

                // In mock mode, check password or accept default "Pass@123" or role default
                bool isValidPass = user.PasswordHash == password ||
                                  password == "Pass@123" ||
                                  password == $"{user.Role}@123" ||
                                  password == "Admin@123";

                if (!isValidPass)
                {
                    return (false, "Invalid credentials. Please verify your password.", null);
                }

                user.LastLoginAt = DateTime.UtcNow;
                return (true, "Authentication successful.", user);
            }
        }

        public (bool Success, string Message, ApplicationUser? User) RegisterPatient(RegisterPatientViewModel model)
        {
            lock (_lock)
            {
                if (_users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return (false, "An account with this Email Address already exists.", null);
                }

                if (_users.Any(u => u.NID == model.NID))
                {
                    return (false, "An account with this National ID (NID) already exists.", null);
                }

                var newUser = new ApplicationUser
                {
                    Id = $"usr-pat-{Guid.NewGuid().ToString()[..8]}",
                    FullName = model.FullName.Trim(),
                    Email = model.Email.Trim().ToLowerInvariant(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    NID = model.NID.Trim(),
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    BloodGroup = model.BloodGroup,
                    District = model.District,
                    Upazila = model.Upazila,
                    Address = model.Address,
                    Role = SystemRoles.Patient,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = model.Password
                };

                _users.Insert(0, newUser);
                return (true, "Registration completed successfully! Welcome to MediCamp.", newUser);
            }
        }

        public (bool Success, string Message, ApplicationUser? User) RegisterHost(RegisterHostViewModel model)
        {
            lock (_lock)
            {
                if (_users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return (false, "An account with this Official Email already exists.", null);
                }

                var newHost = new ApplicationUser
                {
                    Id = $"usr-host-{Guid.NewGuid().ToString()[..8]}",
                    FullName = model.ContactPersonName.Trim(),
                    Email = model.Email.Trim().ToLowerInvariant(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    OrganizationName = model.OrganizationName.Trim(),
                    OrganizationRegNo = model.OrganizationRegNo.Trim(),
                    FocalPersonContact = model.PhoneNumber.Trim(),
                    District = model.District,
                    Upazila = model.Upazila,
                    Address = model.HeadOfficeAddress,
                    Role = SystemRoles.Host,
                    HostApprovalStatus = "Pending", // Awaiting Admin Approval
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = model.Password
                };

                _users.Insert(0, newHost);
                return (true, "Registration submitted! Your NGO application has been received and is pending Admin verification.", newHost);
            }
        }

        public (bool Success, string Message, ApplicationUser? User) RegisterUser(RegisterViewModel model)
        {
            lock (_lock)
            {
                if (_users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return (false, "An account with this Email Address already exists.", null);
                }

                if (!string.IsNullOrWhiteSpace(model.NID) && _users.Any(u => u.NID == model.NID.Trim()))
                {
                    return (false, "An account with this National ID (NID) already exists.", null);
                }

                string targetRole = SystemRoles.AllRoles.Contains(model.Role) ? model.Role : SystemRoles.Patient;
                string approvalStatus = targetRole == SystemRoles.Host ? "Pending" : "Approved";

                var newUser = new ApplicationUser
                {
                    Id = $"usr-{targetRole.ToLowerInvariant()}-{Guid.NewGuid().ToString()[..8]}",
                    FullName = model.FullName.Trim(),
                    Email = model.Email.Trim().ToLowerInvariant(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    NID = model.NID?.Trim(),
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    BloodGroup = model.BloodGroup,
                    District = model.District,
                    Upazila = model.Upazila,
                    Address = model.Address,
                    MedicalSpecialization = model.MedicalSpecialization,
                    BMDCRegNo = model.BMDCRegNo,
                    OrganizationName = model.OrganizationName,
                    OrganizationRegNo = model.OrganizationRegNo,
                    Role = targetRole,
                    HostApprovalStatus = approvalStatus,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = model.Password
                };

                _users.Insert(0, newUser);

                string successMsg = targetRole switch
                {
                    SystemRoles.Host => "NGO Host registration submitted successfully! Pending Admin verification.",
                    SystemRoles.Doctor => "Doctor registration completed! You can now log into your clinical portal.",
                    SystemRoles.Volunteer => "Volunteer registration completed! You can now sign into your field triage desk.",
                    SystemRoles.Pharmacist => "Pharmacist registration completed! You can now log into your dispensary station.",
                    _ => "Patient registration completed successfully! Welcome to MediCamp."
                };

                return (true, successMsg, newUser);
            }
        }

        public (bool Success, string Message) CreateUser(CreateUserViewModel model)
        {
            lock (_lock)
            {
                if (_users.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return (false, "A user with this Email address already exists.");
                }

                if (!string.IsNullOrWhiteSpace(model.NID) && _users.Any(u => u.NID == model.NID.Trim()))
                {
                    return (false, "A user with this National ID (NID) already exists.");
                }

                var user = new ApplicationUser
                {
                    Id = $"usr-{model.Role.ToLowerInvariant()}-{Guid.NewGuid().ToString()[..8]}",
                    FullName = model.FullName.Trim(),
                    Email = model.Email.Trim().ToLowerInvariant(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    NID = model.NID?.Trim(),
                    Role = model.Role,
                    District = model.District,
                    Upazila = model.Upazila,
                    MedicalSpecialization = model.MedicalSpecialization,
                    BMDCRegNo = model.BMDCRegNo,
                    OrganizationName = model.OrganizationName,
                    HostApprovalStatus = model.Role == SystemRoles.Host ? "Approved" : "Approved",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = model.TemporaryPassword
                };

                _users.Insert(0, user);
                return (true, $"User '{user.FullName}' successfully registered with role '{user.Role}'.");
            }
        }

        public (bool Success, string Message) UpdateUserRole(string userId, string newRole)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                if (user == null) return (false, "User not found.");

                if (!SystemRoles.AllRoles.Contains(newRole))
                {
                    return (false, "Invalid role specified.");
                }

                string oldRole = user.Role;
                user.Role = newRole;
                return (true, $"Role for '{user.FullName}' updated from {oldRole} to {newRole}.");
            }
        }

        public (bool Success, string Message) ToggleUserStatus(string userId)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                if (user == null) return (false, "User not found.");

                user.IsActive = !user.IsActive;
                string statusText = user.IsActive ? "Activated" : "Disabled";
                return (true, $"User account '{user.FullName}' has been {statusText}.");
            }
        }

        public (bool Success, string Message) ResetUserPassword(string userId, string newPassword)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                if (user == null) return (false, "User not found.");

                user.PasswordHash = newPassword;
                return (true, $"Password for '{user.FullName}' was successfully reset to: {newPassword}");
            }
        }

        public (bool Success, string Message) DeleteUser(string userId)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                if (user == null) return (false, "User not found.");

                _users.Remove(user);
                return (true, $"User '{user.FullName}' was deleted from the system.");
            }
        }

        public HomeLandingViewModel GetHomeLandingData()
        {
            lock (_lock)
            {
                return new HomeLandingViewModel
                {
                    TotalCampsCount = _camps.Count + 18, // dynamic realistic metrics
                    TotalPatientsServed = 14250,
                    TotalDoctorsCount = _users.Count(u => u.Role == SystemRoles.Doctor) + 32,
                    TotalVolunteersCount = _users.Count(u => u.Role == SystemRoles.Volunteer) + 78,
                    FreeMedicinesDispensed = 38400,
                    DistrictsReached = 24,
                    UpcomingCamps = _camps.Where(c => c.Status != "Completed").Take(3).ToList()
                };
            }
        }

        public List<CampOverviewItem> GetAllCamps()
        {
            lock (_lock)
            {
                return _camps.OrderBy(c => c.StartDate).ToList();
            }
        }

        public CampOverviewItem? GetCampById(int id)
        {
            lock (_lock)
            {
                return _camps.FirstOrDefault(c => c.Id == id);
            }
        }
    }
}
