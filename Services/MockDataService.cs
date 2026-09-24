using MediCamp.Data;
using MediCamp.Models;
using MediCamp.Models.Domain;
using MediCamp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MediCamp.Services
{
    /// <summary>
    /// Note: Originally this was an in-memory Mock Data Service.
    /// It has now been upgraded to interact directly with Neon PostgreSQL (ApplicationDbContext)
    /// to support full persistence. The interface name is kept as IMockDataService to avoid breaking DI.
    /// </summary>
    public class MockDataService : IMockDataService
    {
        private readonly ApplicationDbContext _dbContext;

        public MockDataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<ApplicationUser> GetAllUsers()
        {
            return _dbContext.Users.OrderByDescending(u => u.CreatedAt).ToList();
        }

        public List<ApplicationUser> GetFilteredUsers(string? searchTerm, string? role, string? status)
        {
            var query = _dbContext.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(searchTerm) || 
                                         u.Email.ToLower().Contains(searchTerm) || 
                                         (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)) ||
                                         (u.NID != null && u.NID.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(role) && role != "All")
            {
                query = query.Where(u => u.Role == role);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                bool isActive = status == "Active";
                query = query.Where(u => u.IsActive == isActive);
            }

            return query.OrderByDescending(u => u.CreatedAt).ToList();
        }

        public ApplicationUser? GetUserById(string id)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Id == id);
        }

        public ApplicationUser? GetUserByEmailOrNid(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return null;
            var clean = identifier.Trim().ToLowerInvariant();
            return _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == clean || (u.NID != null && u.NID.Trim() == identifier.Trim()));
        }

        public (bool Success, string Message, ApplicationUser? User) Authenticate(string identifier, string password)
        {
            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
                return (false, "Please enter both identifier and password.", null);

            var user = GetUserByEmailOrNid(identifier);
            if (user == null || user.PasswordHash != password.Trim()) 
                return (false, "Invalid credentials.", null);

            if (!user.IsActive) 
                return (false, "Your account has been deactivated by the system administrator.", null);

            if (user.Role == SystemRoles.Host)
            {
                if (user.HostApprovalStatus == "Rejected")
                    return (false, $"Host registration rejected. Reason: {user.HostRejectionReason}", null);
                if (user.HostApprovalStatus == "Pending")
                    return (false, "Your host registration is currently pending review by System Administrators.", null);
            }

            user.LastLoginAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return (true, "Authentication successful.", user);
        }

        public static string GenerateUniquePatientId(ApplicationDbContext dbContext)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            while (true)
            {
                var id = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
                if (!dbContext.Users.Any(u => u.PatientUniqueId == id))
                {
                    return id;
                }
            }
        }

        public (bool Success, string Message, ApplicationUser? User) RegisterPatient(RegisterPatientViewModel model)
        {
            if (_dbContext.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
                return (false, "Email already registered.", null);

            if (!string.IsNullOrWhiteSpace(model.NID) && _dbContext.Users.Any(u => u.NID == model.NID))
                return (false, "NID already registered.", null);

            var newUser = new ApplicationUser
            {
                Id = $"usr-pat-{Guid.NewGuid().ToString()[..8]}",
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                PhoneNumber = model.PhoneNumber.Trim(),
                NID = model.NID?.Trim(),
                DateOfBirth = model.DateOfBirth.HasValue ? DateTime.SpecifyKind(model.DateOfBirth.Value, DateTimeKind.Utc) : null,
                Gender = model.Gender,
                BloodGroup = model.BloodGroup,
                District = model.District,
                Upazila = model.Upazila,
                Address = model.Address?.Trim() ?? string.Empty,
                Role = SystemRoles.Patient,
                PatientUniqueId = GenerateUniquePatientId(_dbContext),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = model.Password
            };

            _dbContext.Users.Add(newUser);

            if (model.IsBloodDonor && !string.IsNullOrEmpty(newUser.BloodGroup))
            {
                var donorProfile = new BloodDonationProfile
                {
                    UserId = newUser.Id,
                    BloodGroup = newUser.BloodGroup,
                    District = newUser.District ?? "Dhaka",
                    Upazila = newUser.Upazila ?? "Dhanmondi",
                    IsAvailableDonor = true,
                    TotalDonationsCount = 0
                };
                _dbContext.BloodDonationProfiles.Add(donorProfile);
            }

            _dbContext.SaveChanges();
            return (true, "Patient registration successful.", newUser);
        }

        public (bool Success, string Message, ApplicationUser? User) RegisterHost(RegisterHostViewModel model)
        {
            if (_dbContext.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
                return (false, "Email already registered.", null);

            string orgName = !string.IsNullOrWhiteSpace(model.OrganizationName) ? model.OrganizationName.Trim() : model.ContactPersonName.Trim();

            var newHost = new ApplicationUser
            {
                Id = $"usr-host-{Guid.NewGuid().ToString()[..8]}",
                FullName = model.ContactPersonName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                PhoneNumber = model.PhoneNumber.Trim(),
                OrganizationName = orgName,
                OrganizationType = string.IsNullOrWhiteSpace(model.OrganizationType) ? "NGO" : model.OrganizationType,
                OrganizationRegNo = model.OrganizationRegNo?.Trim(),
                FocalPersonContact = model.PhoneNumber.Trim(),
                District = model.District,
                Upazila = model.Upazila,
                Address = model.HeadOfficeAddress,
                Role = SystemRoles.Host,
                HostApprovalStatus = "Pending",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = model.Password
            };

            _dbContext.Users.Add(newHost);
            _dbContext.SaveChanges();
            return (true, "Host application received.", newHost);
        }

        public (bool Success, string Message, ApplicationUser? User) RegisterUser(RegisterViewModel model)
        {
            if (_dbContext.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
                return (false, "Email address is already registered.", null);

            string role = !string.IsNullOrWhiteSpace(model.Role) ? model.Role : SystemRoles.Patient;
            var newUser = new ApplicationUser
            {
                Id = $"usr-{role.ToLower()}-{Guid.NewGuid().ToString()[..8]}",
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                PhoneNumber = model.PhoneNumber.Trim(),
                NID = model.NID?.Trim(),
                DateOfBirth = model.DateOfBirth.HasValue ? DateTime.SpecifyKind(model.DateOfBirth.Value, DateTimeKind.Utc) : null,
                Gender = model.Gender ?? "Male",
                BloodGroup = model.BloodGroup ?? "O+",
                District = model.District ?? "Dhaka",
                Upazila = model.Upazila ?? "Dhanmondi",
                Address = model.Address?.Trim() ?? string.Empty,
                Role = role,
                PatientUniqueId = role == SystemRoles.Patient ? GenerateUniquePatientId(_dbContext) : null,
                MedicalSpecialization = role == SystemRoles.Doctor ? model.MedicalSpecialization?.Trim() : null,
                BMDCRegNo = role == SystemRoles.Doctor ? model.BMDCRegNo?.Trim() : null,
                OrganizationName = role == SystemRoles.Host ? (!string.IsNullOrWhiteSpace(model.OrganizationName) ? model.OrganizationName.Trim() : model.FullName.Trim()) : null,
                OrganizationType = role == SystemRoles.Host ? (model.OrganizationType ?? "NGO") : null,
                OrganizationRegNo = role == SystemRoles.Host ? model.OrganizationRegNo?.Trim() : null,
                FocalPersonContact = role == SystemRoles.Host ? (model.FocalPersonContact?.Trim() ?? model.PhoneNumber.Trim()) : null,
                HostApprovalStatus = role == SystemRoles.Host ? "Pending" : "Approved",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = model.Password
            };

            _dbContext.Users.Add(newUser);

            if (role == SystemRoles.Patient && model.IsBloodDonor && !string.IsNullOrEmpty(newUser.BloodGroup))
            {
                var donorProfile = new BloodDonationProfile
                {
                    UserId = newUser.Id,
                    BloodGroup = newUser.BloodGroup,
                    District = newUser.District ?? "Dhaka",
                    Upazila = newUser.Upazila ?? "Dhanmondi",
                    IsAvailableDonor = true,
                    TotalDonationsCount = 0
                };
                _dbContext.BloodDonationProfiles.Add(donorProfile);
            }

            _dbContext.SaveChanges();
            return (true, "User registration successful.", newUser);
        }

        public (bool Success, string Message) UpdateUserProfile(string userId, EditProfileViewModel model)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return (false, "User not found.");
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.NID = model.NID;
            user.DateOfBirth = model.DateOfBirth.HasValue ? DateTime.SpecifyKind(model.DateOfBirth.Value, DateTimeKind.Utc) : null;
            user.Gender = model.Gender;
            user.BloodGroup = model.BloodGroup;
            user.District = model.District;
            user.Upazila = model.Upazila;
            user.Address = model.Address;

            if (user.Role == SystemRoles.Doctor)
            {
                user.MedicalSpecialization = model.MedicalSpecialization;
                user.BMDCRegNo = model.BMDCRegNo;
            }

            if (user.Role == SystemRoles.Host)
            {
                user.OrganizationName = model.OrganizationName;
                user.OrganizationType = model.OrganizationType;
                user.OrganizationRegNo = model.OrganizationRegNo;
                user.FocalPersonContact = model.FocalPersonContact;
            }

            _dbContext.SaveChanges();
            return (true, "Profile updated successfully.");
        }

        public (bool Success, string Message) CreateUser(CreateUserViewModel model)
        {
            if (_dbContext.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
                return (false, "Email already registered.");

            var newUser = new ApplicationUser
            {
                Id = $"usr-{Guid.NewGuid().ToString()[..8]}",
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                Role = model.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = model.TemporaryPassword
            };

            if (model.Role == SystemRoles.Patient)
            {
                newUser.PatientUniqueId = GenerateUniquePatientId(_dbContext);
            }

            if (model.Role == SystemRoles.Host)
            {
                newUser.HostApprovalStatus = "Approved";
                newUser.OrganizationName = model.FullName;
            }

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();
            return (true, "User created successfully.");
        }

        public (bool Success, string Message) UpdateUserRole(string userId, string newRole)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return (false, "User not found.");

            user.Role = newRole;
            _dbContext.SaveChanges();
            return (true, "Role updated successfully.");
        }

        public (bool Success, string Message) ToggleUserStatus(string userId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return (false, "User not found.");

            user.IsActive = !user.IsActive;
            _dbContext.SaveChanges();
            return (true, "User status toggled successfully.");
        }

        public (bool Success, string Message) ResetUserPassword(string userId, string newPassword)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return (false, "User not found.");

            user.PasswordHash = newPassword;
            _dbContext.SaveChanges();
            return (true, "Password reset successfully.");
        }

        public (bool Success, string Message) DeleteUser(string userId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return (false, "User not found.");

            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
            return (true, "User deleted successfully.");
        }

        public (bool Success, string Message) ApproveHost(string userId)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId && u.Role == SystemRoles.Host);
            if (user == null) return (false, "Host organization not found.");

            user.HostApprovalStatus = "Approved";
            user.HostRejectionReason = null;
            _dbContext.SaveChanges();

            return (true, $"Host organization '{user.OrganizationName ?? user.FullName}' has been approved.");
        }

        public (bool Success, string Message) RejectHost(string userId, string rejectionReason)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId && u.Role == SystemRoles.Host);
            if (user == null) return (false, "Host organization not found.");

            user.HostApprovalStatus = "Rejected";
            user.HostRejectionReason = rejectionReason;
            _dbContext.SaveChanges();

            return (true, $"Host organization '{user.OrganizationName ?? user.FullName}' has been rejected.");
        }

        public List<ApplicationUser> GetHostsByStatus(string? status)
        {
            var query = _dbContext.Users.Where(u => u.Role == SystemRoles.Host);

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(u => u.HostApprovalStatus == status);
            }

            return query.OrderByDescending(u => u.CreatedAt).ToList();
        }

        public HomeLandingViewModel GetHomeLandingData()
        {
            var liveCamp = _dbContext.Camps
                .Include(c => c.Host)
                .Where(c => c.Status == "Ongoing")
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefault();

            if (liveCamp == null)
            {
                liveCamp = _dbContext.Camps
                    .Include(c => c.Host)
                    .Where(c => c.Status == "Scheduled")
                    .OrderBy(c => c.StartDate)
                    .FirstOrDefault();
            }

            if (liveCamp == null)
            {
                liveCamp = _dbContext.Camps
                    .Include(c => c.Host)
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefault();
            }

            LiveCampStatusViewModel? liveStatus = null;

            if (liveCamp != null)
            {
                int totalDonors = _dbContext.BloodDonationProfiles.Count(b => b.IsAvailableDonor);
                if (totalDonors == 0)
                {
                    totalDonors = _dbContext.BloodDonationProfiles.Count();
                }
                string donorText = totalDonors > 0 
                    ? (totalDonors >= 1000 ? $"{totalDonors:N0}+" : $"{totalDonors:N0} Active") 
                    : "18,400+";

                int stockOutages = _dbContext.CampInventories
                    .Where(ci => ci.CampId == liveCamp.Id && ci.QuantityAllocated > 0 && ci.QuantityAllocated <= ci.QuantityDispensed)
                    .Count();

                var campReferrals = _dbContext.Referrals
                    .Include(r => r.Consultation)
                        .ThenInclude(c => c!.TriageRecord)
                    .Where(r => r.Consultation != null && r.Consultation.TriageRecord != null && r.Consultation.TriageRecord.CampId == liveCamp.Id)
                    .ToList();

                string refAlertTitle = "Hospital Referral";
                string refAlertMessage;

                if (campReferrals.Any())
                {
                    var urgentCount = campReferrals.Count(r => r.Urgency == "Urgent" || r.Urgency == "Emergency");
                    var displayCount = urgentCount > 0 ? urgentCount : campReferrals.Count;
                    var topHospital = campReferrals.FirstOrDefault()?.ReferredHospital ?? $"{liveCamp.District} Sadar Hospital";
                    refAlertMessage = $"{displayCount} critical patient{(displayCount > 1 ? "s" : "")} routed to {topHospital}.";
                }
                else
                {
                    refAlertTitle = "Hospital Referral";
                    refAlertMessage = $"Direct referral channel connected to {liveCamp.District} Sadar Hospital.";
                }

                int served = liveCamp.ServedPatientsCount;
                int expected = liveCamp.ExpectedPatients > 0 ? liveCamp.ExpectedPatients : (liveCamp.RegisteredPatientsCount > 0 ? liveCamp.RegisteredPatientsCount : 100);
                int progressPercent = (int)Math.Clamp(Math.Round((double)served / expected * 100), 0, 100);

                string queueLabel = liveCamp.Status == "Ongoing" ? "Today's Patient Queue" : "Registration Status";
                string queueFraction = liveCamp.Status == "Ongoing"
                    ? $"{served} / {expected} Served"
                    : $"{liveCamp.RegisteredPatientsCount} / {expected} Registered";

                if (liveCamp.Status != "Ongoing" && liveCamp.RegisteredPatientsCount > 0 && served == 0)
                {
                    progressPercent = (int)Math.Clamp(Math.Round((double)liveCamp.RegisteredPatientsCount / expected * 100), 0, 100);
                }

                string statusBadge = liveCamp.Status switch
                {
                    "Ongoing" => "LIVE SYSTEM STATUS",
                    "Scheduled" => "UPCOMING SPOTLIGHT",
                    "Completed" => "RECENT CAMP REPORT",
                    _ => "CAMP STATUS"
                };

                var pendingTriage = _dbContext.TriageRecords.Count(t => t.CampId == liveCamp.Id && !t.IsSeenByDoctor);
                string waitTime = pendingTriage > 0 ? $"{Math.Max(15, pendingTriage * 4)} mins" : (liveCamp.Status == "Ongoing" ? "22 mins" : "-- mins");

                liveStatus = new LiveCampStatusViewModel
                {
                    CampId = liveCamp.Id,
                    Title = liveCamp.Title,
                    CampType = liveCamp.CampType,
                    Status = liveCamp.Status,
                    StatusBadgeText = statusBadge,
                    HostOrganization = liveCamp.Host != null ? (!string.IsNullOrWhiteSpace(liveCamp.Host.OrganizationName) ? liveCamp.Host.OrganizationName : liveCamp.Host.FullName) : "Friendship Bangladesh Healthcare",
                    Venue = liveCamp.Venue,
                    District = liveCamp.District,
                    Upazila = liveCamp.Upazila,
                    ExpectedPatients = liveCamp.ExpectedPatients,
                    ServedPatientsCount = liveCamp.ServedPatientsCount,
                    RegisteredPatientsCount = liveCamp.RegisteredPatientsCount,
                    ProgressPercentage = progressPercent,
                    QueueLabel = queueLabel,
                    QueueFractionText = queueFraction,
                    AvgWaitTime = waitTime,
                    StockOutagesCount = stockOutages,
                    TotalBloodDonors = totalDonors,
                    BloodDonorsText = donorText,
                    TriageIntakeSpeed = "< 45s",
                    ReferralAlertTitle = refAlertTitle,
                    ReferralAlertMessage = refAlertMessage,
                    HasActiveCamp = true
                };
            }

            return new HomeLandingViewModel
            {
                TotalCampsCount = _dbContext.Camps.Count(),
                TotalPatientsServed = _dbContext.Camps.Sum(c => c.ServedPatientsCount),
                TotalDoctorsCount = _dbContext.Users.Count(u => u.Role == SystemRoles.Doctor),
                TotalVolunteersCount = _dbContext.Users.Count(u => u.Role == SystemRoles.Volunteer),
                FreeMedicinesDispensed = 12500, // Dummy data for now
                DistrictsReached = _dbContext.Camps.Select(c => c.District).Distinct().Count(),
                UpcomingCamps = GetAllCamps().Take(3).ToList(),
                LiveCampStatus = liveStatus
            };
        }

        public List<CampOverviewItem> GetAllCamps()
        {
            return _dbContext.Camps
                .Include(c => c.Host)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CampOverviewItem
                {
                    Id = c.Id,
                    Title = c.Title,
                    CampType = c.CampType,
                    District = c.District,
                    Upazila = c.Upazila,
                    Venue = c.Venue,
                    HostOrganization = c.Host != null ? (c.Host.OrganizationName ?? c.Host.FullName) : "Unknown",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    ExpectedPatients = c.ExpectedPatients,
                    ServedPatientsCount = c.ServedPatientsCount,
                    Status = c.Status
                }).ToList();
        }

        public CampOverviewItem? GetCampById(int id)
        {
            return GetAllCamps().FirstOrDefault(c => c.Id == id);
        }

        public List<CampStaffRequest> GetRequestsForCamp(int campId)
        {
            return _dbContext.CampStaffRequests
                .Include(r => r.Doctor)
                .Where(r => r.CampId == campId)
                .OrderByDescending(r => r.RequestedAt)
                .ToList();
        }

        public List<CampStaffRequest> GetRequestsForDoctor(string doctorId)
        {
            return _dbContext.CampStaffRequests
                .Include(r => r.Camp)
                    .ThenInclude(c => c.Host)
                .Where(r => r.DoctorId == doctorId)
                .OrderByDescending(r => r.RequestedAt)
                .ToList();
        }

        public (bool Success, string Message) SendCampStaffRequest(int campId, string doctorId)
        {
            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId);
            if (camp == null) return (false, "Camp not found.");

            // Check if doctor is already approved in another active camp
            var activeAssignment = _dbContext.CampStaffRequests
                .Include(r => r.Camp)
                .FirstOrDefault(r => r.DoctorId == doctorId && r.Status == "Approved" && r.Camp != null && r.Camp.Status != "Completed" && r.Camp.Status != "Cancelled" && r.Camp.Status != "Rejected");

            if (activeAssignment != null)
            {
                if (activeAssignment.CampId == campId)
                {
                    return (false, "This doctor has already accepted the invitation for this camp.");
                }
                return (false, $"Doctor is currently unavailable. They have already accepted an invitation for '{activeAssignment.Camp?.Title}'.");
            }

            var existingRequest = _dbContext.CampStaffRequests
                .FirstOrDefault(r => r.CampId == campId && r.DoctorId == doctorId);

            if (existingRequest != null)
            {
                return (false, $"A request to this doctor is already {existingRequest.Status}.");
            }

            var newRequest = new CampStaffRequest
            {
                CampId = campId,
                DoctorId = doctorId,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _dbContext.CampStaffRequests.Add(newRequest);
            _dbContext.SaveChanges();

            return (true, "Staff request sent successfully.");
        }

        public (bool Success, string Message) RespondToCampStaffRequest(int requestId, string doctorId, string status)
        {
            var request = _dbContext.CampStaffRequests.Include(r => r.Camp).FirstOrDefault(r => r.Id == requestId && r.DoctorId == doctorId);
            if (request == null)
            {
                return (false, "Request not found.");
            }

            if (request.Status != "Pending")
            {
                return (false, $"Request is already {request.Status}.");
            }

            if (status != "Approved" && status != "Denied")
            {
                return (false, "Invalid status response.");
            }

            if (status == "Approved")
            {
                var activeAssignment = _dbContext.CampStaffRequests
                    .Include(r => r.Camp)
                    .FirstOrDefault(r => r.DoctorId == doctorId && r.Status == "Approved" && r.Id != requestId && r.Camp != null && r.Camp.Status != "Completed" && r.Camp.Status != "Cancelled" && r.Camp.Status != "Rejected");

                if (activeAssignment != null)
                {
                    return (false, $"You have already accepted an invitation for '{activeAssignment.Camp?.Title}'. You cannot accept multiple active camps simultaneously.");
                }
            }

            request.Status = status;
            request.RespondedAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return (true, $"Request {status.ToLower()} successfully.");
        }

        // --- Volunteer Requests ---
        public List<CampVolunteerRequest> GetVolunteerRequestsForCamp(int campId)
        {
            return _dbContext.CampVolunteerRequests
                .Include(r => r.Volunteer)
                .Where(r => r.CampId == campId)
                .ToList();
        }

        public List<CampVolunteerRequest> GetRequestsForVolunteer(string volunteerId)
        {
            return _dbContext.CampVolunteerRequests
                .Include(r => r.Camp)
                .Where(r => r.VolunteerId == volunteerId)
                .OrderByDescending(r => r.RequestedAt)
                .ToList();
        }

        public (bool Success, string Message) SendCampVolunteerRequest(int campId, string volunteerId)
        {
            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId);
            if (camp == null) return (false, "Camp not found.");

            // Check if volunteer is already approved in another active camp
            var activeAssignment = _dbContext.CampVolunteerRequests
                .Include(r => r.Camp)
                .FirstOrDefault(r => r.VolunteerId == volunteerId && r.Status == "Approved" && r.Camp != null && r.Camp.Status != "Completed" && r.Camp.Status != "Cancelled" && r.Camp.Status != "Rejected");

            if (activeAssignment != null)
            {
                if (activeAssignment.CampId == campId)
                {
                    return (false, "This volunteer has already accepted the invitation for this camp.");
                }
                return (false, $"Volunteer is currently unavailable. They have already accepted an invitation for '{activeAssignment.Camp?.Title}'.");
            }

            var existingRequest = _dbContext.CampVolunteerRequests
                .FirstOrDefault(r => r.CampId == campId && r.VolunteerId == volunteerId);

            if (existingRequest != null)
            {
                return (false, "A request has already been sent to this volunteer.");
            }

            var request = new CampVolunteerRequest
            {
                CampId = campId,
                VolunteerId = volunteerId,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _dbContext.CampVolunteerRequests.Add(request);
            _dbContext.SaveChanges();

            return (true, "Invitation sent successfully.");
        }

        public (bool Success, string Message) RespondToCampVolunteerRequest(int requestId, string volunteerId, string status)
        {
            var request = _dbContext.CampVolunteerRequests.Include(r => r.Camp).FirstOrDefault(r => r.Id == requestId && r.VolunteerId == volunteerId);
            if (request == null) return (false, "Request not found.");

            if (request.Status != "Pending") return (false, "Request has already been processed.");

            if (status != "Approved" && status != "Denied")
            {
                return (false, "Invalid status response.");
            }

            if (status == "Approved")
            {
                var activeAssignment = _dbContext.CampVolunteerRequests
                    .Include(r => r.Camp)
                    .FirstOrDefault(r => r.VolunteerId == volunteerId && r.Status == "Approved" && r.Id != requestId && r.Camp != null && r.Camp.Status != "Completed" && r.Camp.Status != "Cancelled" && r.Camp.Status != "Rejected");

                if (activeAssignment != null)
                {
                    return (false, $"You have already accepted an invitation for '{activeAssignment.Camp?.Title}'. You cannot accept multiple active camps simultaneously.");
                }
            }

            request.Status = status;
            request.RespondedAt = DateTime.UtcNow;

            _dbContext.SaveChanges();

            return (true, $"Request {status.ToLower()} successfully.");
        }

        // --- Pharmacist Requests ---
        public List<CampPharmacistRequest> GetPharmacistRequestsForCamp(int campId)
        {
            return _dbContext.CampPharmacistRequests
                .Include(r => r.Pharmacist)
                .Where(r => r.CampId == campId)
                .ToList();
        }

        public List<CampPharmacistRequest> GetRequestsForPharmacist(string pharmacistId)
        {
            return _dbContext.CampPharmacistRequests
                .Include(r => r.Camp)
                .Where(r => r.PharmacistId == pharmacistId)
                .OrderByDescending(r => r.RequestedAt)
                .ToList();
        }

        public (bool Success, string Message) SendCampPharmacistRequest(int campId, string pharmacistId)
        {
            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId);
            if (camp == null) return (false, "Camp not found.");

            // Check if pharmacist is already approved in another active camp
            var activeAssignment = _dbContext.CampPharmacistRequests
                .Include(r => r.Camp)
                .FirstOrDefault(r => r.PharmacistId == pharmacistId && r.Status == "Approved" && r.Camp != null && r.Camp.Status != "Completed" && r.Camp.Status != "Cancelled" && r.Camp.Status != "Rejected");

            if (activeAssignment != null)
            {
                if (activeAssignment.CampId == campId)
                {
                    return (false, "This pharmacist has already accepted the invitation for this camp.");
                }
                return (false, $"Pharmacist is currently unavailable. They have already accepted an invitation for '{activeAssignment.Camp?.Title}'.");
            }

            var existingRequest = _dbContext.CampPharmacistRequests
                .FirstOrDefault(r => r.CampId == campId && r.PharmacistId == pharmacistId);

            if (existingRequest != null)
            {
                return (false, "A request has already been sent to this pharmacist.");
            }

            var request = new CampPharmacistRequest
            {
                CampId = campId,
                PharmacistId = pharmacistId,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _dbContext.CampPharmacistRequests.Add(request);
            _dbContext.SaveChanges();

            return (true, "Invitation sent successfully.");
        }

        public (bool Success, string Message) RespondToCampPharmacistRequest(int requestId, string pharmacistId, string status)
        {
            var request = _dbContext.CampPharmacistRequests.Include(r => r.Camp).FirstOrDefault(r => r.Id == requestId && r.PharmacistId == pharmacistId);
            if (request == null) return (false, "Request not found.");

            if (request.Status != "Pending") return (false, "Request has already been processed.");

            if (status != "Approved" && status != "Denied")
            {
                return (false, "Invalid status response.");
            }

            if (status == "Approved")
            {
                var activeAssignment = _dbContext.CampPharmacistRequests
                    .Include(r => r.Camp)
                    .FirstOrDefault(r => r.PharmacistId == pharmacistId && r.Status == "Approved" && r.Id != requestId && r.Camp != null && r.Camp.Status != "Completed" && r.Camp.Status != "Cancelled" && r.Camp.Status != "Rejected");

                if (activeAssignment != null)
                {
                    return (false, $"You have already accepted an invitation for '{activeAssignment.Camp?.Title}'. You cannot accept multiple active camps simultaneously.");
                }
            }

            request.Status = status;
            request.RespondedAt = DateTime.UtcNow;

            _dbContext.SaveChanges();

            return (true, $"Request {status.ToLower()} successfully.");
        }

        // --- Active Assignments Query Methods ---
        public Dictionary<string, StaffCampAssignmentInfo> GetActiveDoctorAssignments()
        {
            return _dbContext.CampStaffRequests
                .Include(r => r.Camp)
                .Where(r => r.Status == "Approved" && r.Camp != null && r.Camp.Status != "Completed" && r.Camp.Status != "Cancelled" && r.Camp.Status != "Rejected")
                .OrderByDescending(r => r.RequestedAt)
                .AsEnumerable()
                .GroupBy(r => r.DoctorId)
                .ToDictionary(
                    g => g.Key,
                    g => {
                        var r = g.First();
                        return new StaffCampAssignmentInfo
                        {
                            CampId = r.CampId,
                            CampTitle = r.Camp!.Title,
                            CampStatus = r.Camp.Status,
                            StartDate = r.Camp.StartDate,
                            EndDate = r.Camp.EndDate
                        };
                    });
        }

        public Dictionary<string, StaffCampAssignmentInfo> GetActiveVolunteerAssignments()
        {
            return _dbContext.CampVolunteerRequests
                .Include(r => r.Camp)
                .Where(r => r.Status == "Approved" && r.Camp != null && r.Camp.Status != "Completed" && r.Camp.Status != "Cancelled" && r.Camp.Status != "Rejected")
                .OrderByDescending(r => r.RequestedAt)
                .AsEnumerable()
                .GroupBy(r => r.VolunteerId)
                .ToDictionary(
                    g => g.Key,
                    g => {
                        var r = g.First();
                        return new StaffCampAssignmentInfo
                        {
                            CampId = r.CampId,
                            CampTitle = r.Camp!.Title,
                            CampStatus = r.Camp.Status,
                            StartDate = r.Camp.StartDate,
                            EndDate = r.Camp.EndDate
                        };
                    });
        }

        public Dictionary<string, StaffCampAssignmentInfo> GetActivePharmacistAssignments()
        {
            return _dbContext.CampPharmacistRequests
                .Include(r => r.Camp)
                .Where(r => r.Status == "Approved" && r.Camp != null && r.Camp.Status != "Completed" && r.Camp.Status != "Cancelled" && r.Camp.Status != "Rejected")
                .OrderByDescending(r => r.RequestedAt)
                .AsEnumerable()
                .GroupBy(r => r.PharmacistId)
                .ToDictionary(
                    g => g.Key,
                    g => {
                        var r = g.First();
                        return new StaffCampAssignmentInfo
                        {
                            CampId = r.CampId,
                            CampTitle = r.Camp!.Title,
                            CampStatus = r.Camp.Status,
                            StartDate = r.Camp.StartDate,
                            EndDate = r.Camp.EndDate
                        };
                    });
        }
    }
}
