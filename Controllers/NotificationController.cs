using MediCamp.Data;
using MediCamp.Models;
using MediCamp.Models.Domain;
using MediCamp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MediCamp.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public NotificationController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private ApplicationUser? GetCurrentUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ApplicationUser? user = null;
            if (!string.IsNullOrEmpty(userEmail))
            {
                user = _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == userEmail.ToLower());
            }

            if (user == null && !string.IsNullOrEmpty(userId))
            {
                user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            }

            return user;
        }

        private static string FormatTimeAgo(DateTime dt)
        {
            var utcTime = dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
            var span = DateTime.UtcNow - utcTime;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
            return dt.ToString("dd MMM yyyy");
        }

        [HttpGet]
        public IActionResult GetNotifications()
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return Json(new NotificationSummaryViewModel());
            }

            var notifications = new List<NotificationItemViewModel>();
            var role = user.Role;

            if (role == SystemRoles.Host)
            {
                // 1. Host Camps
                var myCampIds = _dbContext.Camps
                    .Where(c => c.HostId == user.Id)
                    .Select(c => c.Id)
                    .ToList();

                // Doctor requests to join host's camps
                var doctorRequests = _dbContext.CampStaffRequests
                    .Include(r => r.Doctor)
                    .Include(r => r.Camp)
                    .Where(r => myCampIds.Contains(r.CampId))
                    .OrderByDescending(r => r.RequestedAt)
                    .Take(8)
                    .ToList();

                foreach (var req in doctorRequests)
                {
                    var isPending = req.Status == "Pending";
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"doc_req_{req.Id}",
                        Type = "DoctorRequest",
                        Title = isPending ? "Doctor Application Received" : $"Doctor Request ({req.Status})",
                        Message = $"Dr. {req.Doctor?.FullName ?? "Doctor"} requested to join '{req.Camp?.Title}' as Medical Staff.",
                        SenderName = req.Doctor?.FullName ?? "Doctor",
                        SenderRole = "Doctor",
                        CampTitle = req.Camp?.Title,
                        CampId = req.CampId,
                        RequestId = req.Id,
                        Timestamp = req.RequestedAt,
                        TimeAgo = FormatTimeAgo(req.RequestedAt),
                        Status = req.Status,
                        CanApproveReject = isPending,
                        ActionUrl = Url.Action("ManageStaff", "Host", new { id = req.CampId }) ?? "#",
                        IconClass = "fa-solid fa-user-doctor",
                        BadgeColor = isPending ? "bg-teal" : "bg-secondary",
                        IsRead = !isPending
                    });
                }

                // Volunteer requests to join host's camps
                var volunteerRequests = _dbContext.CampVolunteerRequests
                    .Include(r => r.Volunteer)
                    .Include(r => r.Camp)
                    .Where(r => myCampIds.Contains(r.CampId))
                    .OrderByDescending(r => r.RequestedAt)
                    .Take(8)
                    .ToList();

                foreach (var req in volunteerRequests)
                {
                    var isPending = req.Status == "Pending";
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"vol_req_{req.Id}",
                        Type = "VolunteerRequest",
                        Title = isPending ? "Volunteer Application Received" : $"Volunteer Request ({req.Status})",
                        Message = $"{req.Volunteer?.FullName ?? "Volunteer"} applied to join '{req.Camp?.Title}' field team.",
                        SenderName = req.Volunteer?.FullName ?? "Volunteer",
                        SenderRole = "Volunteer",
                        CampTitle = req.Camp?.Title,
                        CampId = req.CampId,
                        RequestId = req.Id,
                        Timestamp = req.RequestedAt,
                        TimeAgo = FormatTimeAgo(req.RequestedAt),
                        Status = req.Status,
                        CanApproveReject = isPending,
                        ActionUrl = Url.Action("ManageStaff", "Host", new { id = req.CampId }) ?? "#",
                        IconClass = "fa-solid fa-hand-holding-heart",
                        BadgeColor = isPending ? "bg-warning text-dark" : "bg-secondary",
                        IsRead = !isPending
                    });
                }

                // Pharmacist requests to join host's camps
                var pharmacistRequests = _dbContext.CampPharmacistRequests
                    .Include(r => r.Pharmacist)
                    .Include(r => r.Camp)
                    .Where(r => myCampIds.Contains(r.CampId))
                    .OrderByDescending(r => r.RequestedAt)
                    .Take(8)
                    .ToList();

                foreach (var req in pharmacistRequests)
                {
                    var isPending = req.Status == "Pending";
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"pharma_req_{req.Id}",
                        Type = "PharmacistRequest",
                        Title = isPending ? "Pharmacist Application Received" : $"Pharmacist Request ({req.Status})",
                        Message = $"{req.Pharmacist?.FullName ?? "Pharmacist"} applied to join '{req.Camp?.Title}' dispensary.",
                        SenderName = req.Pharmacist?.FullName ?? "Pharmacist",
                        SenderRole = "Pharmacist",
                        CampTitle = req.Camp?.Title,
                        CampId = req.CampId,
                        RequestId = req.Id,
                        Timestamp = req.RequestedAt,
                        TimeAgo = FormatTimeAgo(req.RequestedAt),
                        Status = req.Status,
                        CanApproveReject = isPending,
                        ActionUrl = Url.Action("ManageStaff", "Host", new { id = req.CampId }) ?? "#",
                        IconClass = "fa-solid fa-prescription-bottle-medical",
                        BadgeColor = isPending ? "bg-info" : "bg-secondary",
                        IsRead = !isPending
                    });
                }

                // Patient Registrations for host's camps
                var patientRegs = _dbContext.CampPatientRegistrations
                    .Include(p => p.Patient)
                    .Include(p => p.Camp)
                    .Where(p => myCampIds.Contains(p.CampId))
                    .OrderByDescending(p => p.RegisteredAt)
                    .Take(5)
                    .ToList();

                foreach (var reg in patientRegs)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"pat_reg_{reg.Id}",
                        Type = "PatientRegistration",
                        Title = "New Patient Registration",
                        Message = $"{reg.Patient?.FullName ?? "A patient"} registered for '{reg.Camp?.Title}'.",
                        SenderName = reg.Patient?.FullName ?? "Patient",
                        SenderRole = "Patient",
                        CampTitle = reg.Camp?.Title,
                        CampId = reg.CampId,
                        Timestamp = reg.RegisteredAt,
                        TimeAgo = FormatTimeAgo(reg.RegisteredAt),
                        Status = reg.Status,
                        CanApproveReject = false,
                        ActionUrl = Url.Action("Dashboard", "Host") ?? "#",
                        IconClass = "fa-solid fa-user-plus",
                        BadgeColor = "bg-primary",
                        IsRead = true
                    });
                }

                // Host Verification Alert if pending
                if (user.HostApprovalStatus == "Pending")
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = "host_pending_approval",
                        Type = "SystemAlert",
                        Title = "Host Verification In Review",
                        Message = "Your host organization registration is currently undergoing administrator verification.",
                        SenderName = "MediCamp System",
                        SenderRole = "Admin",
                        Timestamp = user.CreatedAt,
                        TimeAgo = FormatTimeAgo(user.CreatedAt),
                        Status = "In Review",
                        CanApproveReject = false,
                        ActionUrl = Url.Action("Dashboard", "Host") ?? "#",
                        IconClass = "fa-solid fa-shield-halved",
                        BadgeColor = "bg-warning text-dark",
                        IsRead = false
                    });
                }
            }
            else if (role == SystemRoles.Admin)
            {
                // 1. Pending Host Registrations
                var pendingHosts = _dbContext.Users
                    .Where(u => u.Role == SystemRoles.Host && u.HostApprovalStatus == "Pending")
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToList();

                foreach (var h in pendingHosts)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"host_appr_{h.Id}",
                        Type = "HostApproval",
                        Title = "Host Organization Approval",
                        Message = $"{h.OrganizationName ?? h.FullName} registered as NGO Host and awaits verification.",
                        SenderName = h.FullName,
                        SenderRole = "Host",
                        Timestamp = h.CreatedAt,
                        TimeAgo = FormatTimeAgo(h.CreatedAt),
                        Status = "Pending",
                        CanApproveReject = false,
                        ActionUrl = Url.Action("HostApprovals", "Admin") ?? "#",
                        IconClass = "fa-solid fa-building-ngo",
                        BadgeColor = "bg-warning text-dark",
                        IsRead = false
                    });
                }

                // 2. Pending Camps
                var pendingCamps = _dbContext.Camps
                    .Include(c => c.Host)
                    .Where(c => c.Status == "Pending Admin Approval")
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToList();

                foreach (var c in pendingCamps)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"camp_appr_{c.Id}",
                        Type = "CampApproval",
                        Title = "Camp Awaiting Approval",
                        Message = $"'{c.Title}' ({c.District}) submitted by {c.Host?.OrganizationName ?? "Host"}.",
                        SenderName = c.Host?.OrganizationName ?? "Host",
                        SenderRole = "Host",
                        CampTitle = c.Title,
                        CampId = c.Id,
                        Timestamp = c.CreatedAt,
                        TimeAgo = FormatTimeAgo(c.CreatedAt),
                        Status = "Pending",
                        CanApproveReject = false,
                        ActionUrl = Url.Action("CampApprovals", "Admin") ?? "#",
                        IconClass = "fa-solid fa-tent",
                        BadgeColor = "bg-danger",
                        IsRead = false
                    });
                }

                // 3. Urgent Blood Requests
                var urgentBlood = _dbContext.BloodRequests
                    .Include(b => b.Requester)
                    .Where(b => b.Status == "Urgent" || b.Status == "Open")
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToList();

                foreach (var b in urgentBlood)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"blood_req_{b.Id}",
                        Type = "BloodRequest",
                        Title = "Urgent Blood Request",
                        Message = $"Urgent {b.BloodGroup} ({b.UnitsRequired} units) requested at {b.HospitalName}.",
                        SenderName = b.Requester?.FullName ?? "Requester",
                        SenderRole = "Patient",
                        Timestamp = b.CreatedAt,
                        TimeAgo = FormatTimeAgo(b.CreatedAt),
                        Status = b.Status,
                        CanApproveReject = false,
                        ActionUrl = Url.Action("BloodRequests", "Admin") ?? "#",
                        IconClass = "fa-solid fa-droplet",
                        BadgeColor = "bg-danger",
                        IsRead = false
                    });
                }
            }
            else if (role == SystemRoles.Doctor)
            {
                // Camp staff requests for this doctor
                var myRequests = _dbContext.CampStaffRequests
                    .Include(r => r.Camp)
                        .ThenInclude(c => c!.Host)
                    .Where(r => r.DoctorId == user.Id)
                    .OrderByDescending(r => r.RequestedAt)
                    .Take(8)
                    .ToList();

                foreach (var req in myRequests)
                {
                    var isPending = req.Status == "Pending";
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"doc_inv_{req.Id}",
                        Type = "StaffInvitation",
                        Title = isPending ? "Camp Staff Invitation" : $"Camp Application ({req.Status})",
                        Message = isPending 
                            ? $"You have an invitation/request for '{req.Camp?.Title}' ({req.Camp?.District})."
                            : $"Your application for '{req.Camp?.Title}' has been {req.Status.ToLower()}.",
                        SenderName = req.Camp?.Host?.OrganizationName ?? "Camp Host",
                        SenderRole = "Host",
                        CampTitle = req.Camp?.Title,
                        CampId = req.CampId,
                        RequestId = req.Id,
                        Timestamp = req.RequestedAt,
                        TimeAgo = FormatTimeAgo(req.RequestedAt),
                        Status = req.Status,
                        CanApproveReject = false,
                        ActionUrl = Url.Action("Requests", "Doctor") ?? "#",
                        IconClass = "fa-solid fa-stethoscope",
                        BadgeColor = isPending ? "bg-teal" : "bg-success",
                        IsRead = !isPending
                    });
                }
            }
            else if (role == SystemRoles.Volunteer)
            {
                var myRequests = _dbContext.CampVolunteerRequests
                    .Include(r => r.Camp)
                    .Where(r => r.VolunteerId == user.Id)
                    .OrderByDescending(r => r.RequestedAt)
                    .Take(8)
                    .ToList();

                foreach (var req in myRequests)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"vol_inv_{req.Id}",
                        Type = "VolunteerInvitation",
                        Title = $"Camp Volunteer Request ({req.Status})",
                        Message = $"Volunteer status for '{req.Camp?.Title}' is {req.Status.ToLower()}.",
                        SenderName = req.Camp?.Title ?? "Camp Host",
                        SenderRole = "Host",
                        CampTitle = req.Camp?.Title,
                        CampId = req.CampId,
                        RequestId = req.Id,
                        Timestamp = req.RequestedAt,
                        TimeAgo = FormatTimeAgo(req.RequestedAt),
                        Status = req.Status,
                        CanApproveReject = false,
                        ActionUrl = Url.Action("Requests", "Volunteer") ?? "#",
                        IconClass = "fa-solid fa-hand-holding-heart",
                        BadgeColor = req.Status == "Pending" ? "bg-warning text-dark" : "bg-success",
                        IsRead = req.Status != "Pending"
                    });
                }
            }
            else if (role == SystemRoles.Pharmacist)
            {
                var myRequests = _dbContext.CampPharmacistRequests
                    .Include(r => r.Camp)
                    .Where(r => r.PharmacistId == user.Id)
                    .OrderByDescending(r => r.RequestedAt)
                    .Take(8)
                    .ToList();

                foreach (var req in myRequests)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"pharma_inv_{req.Id}",
                        Type = "PharmacistInvitation",
                        Title = $"Camp Pharmacist Request ({req.Status})",
                        Message = $"Pharmacy deployment for '{req.Camp?.Title}' is {req.Status.ToLower()}.",
                        SenderName = req.Camp?.Title ?? "Camp Host",
                        SenderRole = "Host",
                        CampTitle = req.Camp?.Title,
                        CampId = req.CampId,
                        RequestId = req.Id,
                        Timestamp = req.RequestedAt,
                        TimeAgo = FormatTimeAgo(req.RequestedAt),
                        Status = req.Status,
                        CanApproveReject = false,
                        ActionUrl = Url.Action("Requests", "Pharmacist") ?? "#",
                        IconClass = "fa-solid fa-pills",
                        BadgeColor = req.Status == "Pending" ? "bg-info" : "bg-success",
                        IsRead = req.Status != "Pending"
                    });
                }
            }
            else if (role == SystemRoles.Patient)
            {
                // Camp Registrations
                var myRegistrations = _dbContext.CampPatientRegistrations
                    .Include(r => r.Camp)
                    .Where(r => r.PatientId == user.Id)
                    .OrderByDescending(r => r.RegisteredAt)
                    .Take(5)
                    .ToList();

                foreach (var reg in myRegistrations)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"pat_camp_{reg.Id}",
                        Type = "PatientCamp",
                        Title = "Camp Registration Confirmed",
                        Message = $"You are registered for '{reg.Camp?.Title}'.",
                        SenderName = reg.Camp?.Title ?? "MediCamp",
                        SenderRole = "Camp",
                        CampTitle = reg.Camp?.Title,
                        CampId = reg.CampId,
                        Timestamp = reg.RegisteredAt,
                        TimeAgo = FormatTimeAgo(reg.RegisteredAt),
                        Status = reg.Status,
                        CanApproveReject = false,
                        ActionUrl = Url.Action("History", "Patient") ?? "#",
                        IconClass = "fa-solid fa-calendar-check",
                        BadgeColor = "bg-success",
                        IsRead = true
                    });
                }
            }

            // Order by timestamp descending
            var sortedNotifications = notifications.OrderByDescending(n => n.Timestamp).ToList();
            var unreadCount = sortedNotifications.Count(n => !n.IsRead);

            var result = new NotificationSummaryViewModel
            {
                UnreadCount = unreadCount,
                Notifications = sortedNotifications
            };

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RespondToStaffRequest(int requestId, string status)
        {
            var user = GetCurrentUser();
            if (user == null) return Json(new { success = false, message = "Unauthorized" });

            var req = _dbContext.CampStaffRequests
                .Include(r => r.Camp)
                .Include(r => r.Doctor)
                .FirstOrDefault(r => r.Id == requestId);

            if (req == null) return Json(new { success = false, message = "Request not found." });

            // Ensure caller is either the Host of the camp or the Doctor
            if (req.Camp != null && req.Camp.HostId != user.Id && req.DoctorId != user.Id && user.Role != SystemRoles.Admin)
            {
                return Json(new { success = false, message = "Access denied." });
            }

            if (status != "Approved" && status != "Denied")
            {
                return Json(new { success = false, message = "Invalid status." });
            }

            req.Status = status;
            req.RespondedAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return Json(new { success = true, message = $"Doctor request {status.ToLower()} successfully!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RespondToVolunteerRequest(int requestId, string status)
        {
            var user = GetCurrentUser();
            if (user == null) return Json(new { success = false, message = "Unauthorized" });

            var req = _dbContext.CampVolunteerRequests
                .Include(r => r.Camp)
                .Include(r => r.Volunteer)
                .FirstOrDefault(r => r.Id == requestId);

            if (req == null) return Json(new { success = false, message = "Request not found." });

            if (req.Camp != null && req.Camp.HostId != user.Id && req.VolunteerId != user.Id && user.Role != SystemRoles.Admin)
            {
                return Json(new { success = false, message = "Access denied." });
            }

            if (status != "Approved" && status != "Denied")
            {
                return Json(new { success = false, message = "Invalid status." });
            }

            req.Status = status;
            req.RespondedAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return Json(new { success = true, message = $"Volunteer request {status.ToLower()} successfully!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RespondToPharmacistRequest(int requestId, string status)
        {
            var user = GetCurrentUser();
            if (user == null) return Json(new { success = false, message = "Unauthorized" });

            var req = _dbContext.CampPharmacistRequests
                .Include(r => r.Camp)
                .Include(r => r.Pharmacist)
                .FirstOrDefault(r => r.Id == requestId);

            if (req == null) return Json(new { success = false, message = "Request not found." });

            if (req.Camp != null && req.Camp.HostId != user.Id && req.PharmacistId != user.Id && user.Role != SystemRoles.Admin)
            {
                return Json(new { success = false, message = "Access denied." });
            }

            if (status != "Approved" && status != "Denied")
            {
                return Json(new { success = false, message = "Invalid status." });
            }

            req.Status = status;
            req.RespondedAt = DateTime.UtcNow;
            _dbContext.SaveChanges();

            return Json(new { success = true, message = $"Pharmacist request {status.ToLower()} successfully!" });
        }
    }
}
