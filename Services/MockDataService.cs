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
                                         (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
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
            return _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == identifier.ToLower() || u.NID == identifier);
        }

        public (bool Success, string Message, ApplicationUser? User) Authenticate(string identifier, string password)
        {
            var user = GetUserByEmailOrNid(identifier);
            if (user == null || user.PasswordHash != password) 
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
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                BloodGroup = model.BloodGroup,
                District = model.District,
                Upazila = model.Upazila,
                Role = SystemRoles.Patient,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = model.Password
            };

            _dbContext.Users.Add(newUser);
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
                return (false, "Email already registered.", null);

            var newUser = new ApplicationUser
            {
                Id = $"usr-gen-{Guid.NewGuid().ToString()[..8]}",
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Role = model.Role ?? SystemRoles.Patient,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = model.Password
            };

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();
            return (true, "User registration successful.", newUser);
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
            return new HomeLandingViewModel
            {
                TotalCampsCount = _dbContext.Camps.Count(),
                TotalPatientsServed = _dbContext.Camps.Sum(c => c.ServedPatientsCount),
                TotalDoctorsCount = _dbContext.Users.Count(u => u.Role == SystemRoles.Doctor),
                TotalVolunteersCount = _dbContext.Users.Count(u => u.Role == SystemRoles.Volunteer),
                FreeMedicinesDispensed = 12500, // Dummy data for now
                DistrictsReached = _dbContext.Camps.Select(c => c.District).Distinct().Count(),
                UpcomingCamps = GetAllCamps().Take(3).ToList()
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
            var request = _dbContext.CampStaffRequests.FirstOrDefault(r => r.Id == requestId && r.DoctorId == doctorId);
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
            var request = _dbContext.CampVolunteerRequests.FirstOrDefault(r => r.Id == requestId && r.VolunteerId == volunteerId);
            if (request == null) return (false, "Request not found.");

            if (request.Status != "Pending") return (false, "Request has already been processed.");

            request.Status = status;
            request.RespondedAt = DateTime.UtcNow;

            _dbContext.SaveChanges();

            return (true, $"Request {status.ToLower()} successfully.");
        }
    }
}
