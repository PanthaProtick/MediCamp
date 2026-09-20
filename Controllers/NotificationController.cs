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

            // =========================================================================
            // 1. ADMIN NOTIFICATIONS (Governance & System Approvals Only)
            // =========================================================================
            if (role == SystemRoles.Admin)
            {
                // A. Pending Host Organization Registrations
                var pendingHosts = _dbContext.Users
                    .Where(u => u.Role == SystemRoles.Host && u.HostApprovalStatus == "Pending")
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(10)
                    .ToList();

                foreach (var h in pendingHosts)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"admin_host_appr_{h.Id}",
                        Type = "HostApproval",
                        Title = "Host Verification Required",
                        Message = $"{h.OrganizationName ?? h.FullName} registered as NGO Host and awaits admin verification.",
                        SenderName = h.FullName,
                        SenderRole = "Host",
                        Timestamp = h.CreatedAt,
                        TimeAgo = FormatTimeAgo(h.CreatedAt),
                        Status = "Pending",
                        CanApproveReject = false,
                        ActionUrl = Url.Action("HostApprovals", "Admin") ?? "/Admin/HostApprovals",
                        IconClass = "fa-solid fa-building-ngo",
                        BadgeColor = "bg-warning text-dark",
                        IsRead = false
                    });
                }

                // B. Pending Camp Applications submitted by Hosts
                var pendingCamps = _dbContext.Camps
                    .Include(c => c.Host)
                    .Where(c => c.Status == "Pending Admin Approval")
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(10)
                    .ToList();

                foreach (var c in pendingCamps)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"admin_camp_appr_{c.Id}",
                        Type = "CampApproval",
                        Title = "Camp Awaiting Approval",
                        Message = $"'{c.Title}' ({c.District}) submitted by {c.Host?.OrganizationName ?? c.Host?.FullName ?? "Host"}.",
                        SenderName = c.Host?.OrganizationName ?? "Host",
                        SenderRole = "Host",
                        CampTitle = c.Title,
                        CampId = c.Id,
                        Timestamp = c.CreatedAt,
                        TimeAgo = FormatTimeAgo(c.CreatedAt),
                        Status = "Pending",
                        CanApproveReject = false,
                        ActionUrl = Url.Action("CampApprovals", "Admin") ?? "/Admin/CampApprovals",
                        IconClass = "fa-solid fa-tent",
                        BadgeColor = "bg-danger",
                        IsRead = false
                    });
                }
            }
            // =========================================================================
            // 2. HOST NOTIFICATIONS (Only for Camps Owned by Current Host & Host Status)
            // =========================================================================
            else if (role == SystemRoles.Host)
            {
                // A. Host's Own Account Approval Status
                if (user.HostApprovalStatus == "Pending")
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = "host_pending_approval",
                        Type = "SystemAlert",
                        Title = "Host Account In Review",
                        Message = "Your organization verification is currently under review by system administrators.",
                        SenderName = "MediCamp Admin",
                        SenderRole = "Admin",
                        Timestamp = user.CreatedAt,
                        TimeAgo = FormatTimeAgo(user.CreatedAt),
                        Status = "In Review",
                        CanApproveReject = false,
                        ActionUrl = Url.Action("Dashboard", "Host") ?? "/Host/Dashboard",
                        IconClass = "fa-solid fa-shield-halved",
                        BadgeColor = "bg-warning text-dark",
                        IsRead = false
                    });
                }
                else if (user.HostApprovalStatus == "Rejected")
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = "host_rejected_approval",
                        Type = "SystemAlert",
                        Title = "Host Registration Rejected",
                        Message = $"Verification not approved: {user.HostRejectionReason ?? "Please contact support."}",
                        SenderName = "MediCamp Admin",
                        SenderRole = "Admin",
                        Timestamp = DateTime.UtcNow,
                        TimeAgo = "Recent",
                        Status = "Rejected",
                        CanApproveReject = false,
                        ActionUrl = Url.Action("Dashboard", "Host") ?? "/Host/Dashboard",
                        IconClass = "fa-solid fa-triangle-exclamation",
                        BadgeColor = "bg-danger",
                        IsRead = false
                    });
                }

                // Camps owned by this specific Host
                var myCampIds = _dbContext.Camps
                    .Where(c => c.HostId == user.Id)
                    .Select(c => c.Id)
                    .ToList();

                // B. Doctor Staff Applications to this host's camps
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
                        ActionUrl = Url.Action("ManageStaff", "Host", new { id = req.CampId }) ?? $"/Host/ManageStaff/{req.CampId}",
                        IconClass = "fa-solid fa-user-doctor",
                        BadgeColor = isPending ? "bg-teal" : "bg-secondary",
                        IsRead = !isPending
                    });
                }

                // C. Volunteer Applications to this host's camps
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
                        ActionUrl = Url.Action("ManageStaff", "Host", new { id = req.CampId }) ?? $"/Host/ManageStaff/{req.CampId}",
                        IconClass = "fa-solid fa-hand-holding-heart",
                        BadgeColor = isPending ? "bg-warning text-dark" : "bg-secondary",
                        IsRead = !isPending
                    });
                }

                // D. Pharmacist Applications to this host's camps
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
                        ActionUrl = Url.Action("ManageStaff", "Host", new { id = req.CampId }) ?? $"/Host/ManageStaff/{req.CampId}",
                        IconClass = "fa-solid fa-prescription-bottle-medical",
                        BadgeColor = isPending ? "bg-info" : "bg-secondary",
                        IsRead = !isPending
                    });
                }

                // E. Patient Registrations for this host's camps
                var patientRegs = _dbContext.CampPatientRegistrations
                    .Include(p => p.Patient)
                    .Include(p => p.Camp)
                    .Where(p => myCampIds.Contains(p.CampId))
                    .OrderByDescending(p => p.RegisteredAt)
                    .Take(6)
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
                        ActionUrl = Url.Action("Dashboard", "Host") ?? "/Host/Dashboard",
                        IconClass = "fa-solid fa-user-plus",
                        BadgeColor = "bg-primary",
                        IsRead = true
                    });
                }
            }
            // =========================================================================
            // 3. DOCTOR NOTIFICATIONS (Direct Invitations & Assigned Emergency Queues)
            // =========================================================================
            else if (role == SystemRoles.Doctor)
            {
                // A. Camp Staff Requests specifically for this Doctor
                var myRequests = _dbContext.CampStaffRequests
                    .Include(r => r.Camp)
                        .ThenInclude(c => c!.Host)
                    .Where(r => r.DoctorId == user.Id)
                    .OrderByDescending(r => r.RequestedAt)
                    .Take(10)
                    .ToList();

                foreach (var req in myRequests)
                {
                    var isPending = req.Status == "Pending";
                    var isApproved = req.Status == "Approved";
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"doc_inv_{req.Id}",
                        Type = "StaffInvitation",
                        Title = isPending ? "Camp Staff Invitation" : $"Application ({req.Status})",
                        Message = isPending
                            ? $"You have an invitation to join '{req.Camp?.Title}' ({req.Camp?.District}) as Medical Staff."
                            : (isApproved
                                ? $"Your appointment for '{req.Camp?.Title}' has been approved by the host."
                                : $"Your request for '{req.Camp?.Title}' was declined."),
                        SenderName = req.Camp?.Host?.OrganizationName ?? "Camp Host",
                        SenderRole = "Host",
                        CampTitle = req.Camp?.Title,
                        CampId = req.CampId,
                        RequestId = req.Id,
                        Timestamp = req.RequestedAt,
                        TimeAgo = FormatTimeAgo(req.RequestedAt),
                        Status = req.Status,
                        CanApproveReject = false,
                        ActionUrl = Url.Action("Requests", "Doctor") ?? "/Doctor/Requests",
                        IconClass = "fa-solid fa-stethoscope",
                        BadgeColor = isPending ? "bg-teal" : (isApproved ? "bg-success" : "bg-secondary"),
                        IsRead = !isPending
                    });
                }

                // B. Urgent Triage Patients in Doctor's Assigned Camps
                var approvedCampIds = myRequests
                    .Where(r => r.Status == "Approved")
                    .Select(r => r.CampId)
                    .Distinct()
                    .ToList();

                if (approvedCampIds.Count > 0)
                {
                    var urgentTriage = _dbContext.TriageRecords
                        .Include(t => t.Camp)
                        .Include(t => t.Patient)
                        .Where(t => approvedCampIds.Contains(t.CampId) && !t.IsSeenByDoctor && t.UrgencyLevel == "Emergency")
                        .OrderByDescending(t => t.RecordedAt)
                        .Take(5)
                        .ToList();

                    foreach (var t in urgentTriage)
                    {
                        notifications.Add(new NotificationItemViewModel
                        {
                            Id = $"doc_urg_triage_{t.Id}",
                            Type = "UrgentTriage",
                            Title = "Emergency Patient in Queue",
                            Message = $"Patient {t.Patient?.FullName ?? "Patient"} (Token #{t.TokenNumber}) flagged as Emergency at '{t.Camp?.Title}'.",
                            SenderName = "Triage Desk",
                            SenderRole = "Volunteer",
                            CampTitle = t.Camp?.Title,
                            CampId = t.CampId,
                            Timestamp = t.RecordedAt,
                            TimeAgo = FormatTimeAgo(t.RecordedAt),
                            Status = "Emergency",
                            CanApproveReject = false,
                            ActionUrl = Url.Action("Queue", "Doctor", new { activeCampId = t.CampId }) ?? "/Doctor/Dashboard",
                            IconClass = "fa-solid fa-triangle-exclamation",
                            BadgeColor = "bg-danger",
                            IsRead = false
                        });
                    }
                }
            }
            // =========================================================================
            // 4. VOLUNTEER NOTIFICATIONS (Direct Volunteer Invitations & Assigned Tasks)
            // =========================================================================
            else if (role == SystemRoles.Volunteer)
            {
                // A. Volunteer Requests specifically for this Volunteer
                var myRequests = _dbContext.CampVolunteerRequests
                    .Include(r => r.Camp)
                        .ThenInclude(c => c!.Host)
                    .Where(r => r.VolunteerId == user.Id)
                    .OrderByDescending(r => r.RequestedAt)
                    .Take(10)
                    .ToList();

                foreach (var req in myRequests)
                {
                    var isPending = req.Status == "Pending";
                    var isApproved = req.Status == "Approved";
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"vol_inv_{req.Id}",
                        Type = "VolunteerInvitation",
                        Title = isPending ? "Field Volunteer Invitation" : $"Volunteer Request ({req.Status})",
                        Message = isPending
                            ? $"You are invited to join '{req.Camp?.Title}' ({req.Camp?.District}) field team."
                            : (isApproved
                                ? $"You are confirmed as a field volunteer for '{req.Camp?.Title}'."
                                : $"Volunteer application for '{req.Camp?.Title}' was updated."),
                        SenderName = req.Camp?.Host?.OrganizationName ?? req.Camp?.Title ?? "Camp Host",
                        SenderRole = "Host",
                        CampTitle = req.Camp?.Title,
                        CampId = req.CampId,
                        RequestId = req.Id,
                        Timestamp = req.RequestedAt,
                        TimeAgo = FormatTimeAgo(req.RequestedAt),
                        Status = req.Status,
                        CanApproveReject = false,
                        ActionUrl = Url.Action("Requests", "Volunteer") ?? "/Volunteer/Requests",
                        IconClass = "fa-solid fa-hand-holding-heart",
                        BadgeColor = isPending ? "bg-warning text-dark" : (isApproved ? "bg-success" : "bg-secondary"),
                        IsRead = !isPending
                    });
                }

                // B. Patient Follow-ups for Volunteer's Approved Camps
                var approvedVolCampIds = myRequests
                    .Where(r => r.Status == "Approved")
                    .Select(r => r.CampId)
                    .Distinct()
                    .ToList();

                if (approvedVolCampIds.Count > 0)
                {
                    var pendingFollowUps = _dbContext.PatientFollowUps
                        .Include(f => f.Camp)
                        .Include(f => f.Patient)
                        .Where(f => approvedVolCampIds.Contains(f.CampId) && f.Status == "Pending")
                        .OrderByDescending(f => f.CreatedAt)
                        .Take(4)
                        .ToList();

                    foreach (var f in pendingFollowUps)
                    {
                        notifications.Add(new NotificationItemViewModel
                        {
                            Id = $"vol_followup_{f.Id}",
                            Type = "PatientFollowUp",
                            Title = "Patient Follow-up Task",
                            Message = $"Follow-up needed for patient {f.Patient?.FullName ?? "Patient"} ({f.Camp?.Title}): {f.Reason}.",
                            SenderName = "Medical Team",
                            SenderRole = "Doctor",
                            CampTitle = f.Camp?.Title,
                            CampId = f.CampId,
                            Timestamp = f.CreatedAt,
                            TimeAgo = FormatTimeAgo(f.CreatedAt),
                            Status = "Pending",
                            CanApproveReject = false,
                            ActionUrl = Url.Action("FollowUps", "Volunteer", new { campId = f.CampId }) ?? "/Volunteer/Dashboard",
                            IconClass = "fa-solid fa-phone-volume",
                            BadgeColor = "bg-info",
                            IsRead = false
                        });
                    }
                }
            }
            // =========================================================================
            // 5. PHARMACIST NOTIFICATIONS (Direct Pharmacy Invitations & Dispensary Tasks)
            // =========================================================================
            else if (role == SystemRoles.Pharmacist)
            {
                // A. Pharmacist Requests specifically for this Pharmacist
                var myRequests = _dbContext.CampPharmacistRequests
                    .Include(r => r.Camp)
                        .ThenInclude(c => c!.Host)
                    .Where(r => r.PharmacistId == user.Id)
                    .OrderByDescending(r => r.RequestedAt)
                    .Take(10)
                    .ToList();

                foreach (var req in myRequests)
                {
                    var isPending = req.Status == "Pending";
                    var isApproved = req.Status == "Approved";
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"pharma_inv_{req.Id}",
                        Type = "PharmacistInvitation",
                        Title = isPending ? "Dispensary Deployment Invitation" : $"Pharmacist Request ({req.Status})",
                        Message = isPending
                            ? $"You have an invitation for '{req.Camp?.Title}' pharmacy dispensary."
                            : (isApproved
                                ? $"You are confirmed as Pharmacist for '{req.Camp?.Title}' dispensary."
                                : $"Dispensary application for '{req.Camp?.Title}' was updated."),
                        SenderName = req.Camp?.Host?.OrganizationName ?? req.Camp?.Title ?? "Camp Host",
                        SenderRole = "Host",
                        CampTitle = req.Camp?.Title,
                        CampId = req.CampId,
                        RequestId = req.Id,
                        Timestamp = req.RequestedAt,
                        TimeAgo = FormatTimeAgo(req.RequestedAt),
                        Status = req.Status,
                        CanApproveReject = false,
                        ActionUrl = Url.Action("Requests", "Pharmacist") ?? "/Pharmacist/Requests",
                        IconClass = "fa-solid fa-pills",
                        BadgeColor = isPending ? "bg-info" : (isApproved ? "bg-success" : "bg-secondary"),
                        IsRead = !isPending
                    });
                }

                // B. Prescriptions awaiting dispensation in Pharmacist's assigned camps
                var approvedPharmaCampIds = myRequests
                    .Where(r => r.Status == "Approved")
                    .Select(r => r.CampId)
                    .Distinct()
                    .ToList();

                if (approvedPharmaCampIds.Count > 0)
                {
                    var pendingPrescriptions = _dbContext.Prescriptions
                        .Include(p => p.Consultation)
                            .ThenInclude(c => c!.TriageRecord)
                                .ThenInclude(t => t!.Camp)
                        .Where(p => !p.IsDispensed && p.Consultation != null && p.Consultation.TriageRecord != null && approvedPharmaCampIds.Contains(p.Consultation.TriageRecord.CampId))
                        .OrderByDescending(p => p.Id)
                        .Take(5)
                        .ToList();

                    foreach (var p in pendingPrescriptions)
                    {
                        notifications.Add(new NotificationItemViewModel
                        {
                            Id = $"pharma_disp_{p.Id}",
                            Type = "PrescriptionQueue",
                            Title = "Prescription Awaiting Dispensation",
                            Message = $"Prescription #{p.Id} queued for dispensing at '{p.Consultation?.TriageRecord?.Camp?.Title}'.",
                            SenderName = "Doctor Consultation",
                            SenderRole = "Doctor",
                            CampTitle = p.Consultation?.TriageRecord?.Camp?.Title,
                            CampId = p.Consultation?.TriageRecord?.CampId,
                            Timestamp = p.Consultation?.ConsultedAt ?? DateTime.UtcNow,
                            TimeAgo = FormatTimeAgo(p.Consultation?.ConsultedAt ?? DateTime.UtcNow),
                            Status = "Pending",
                            CanApproveReject = false,
                            ActionUrl = Url.Action("PrescriptionQueue", "Pharmacist", new { campId = p.Consultation?.TriageRecord?.CampId }) ?? "/Pharmacist/Dashboard",
                            IconClass = "fa-solid fa-capsules",
                            BadgeColor = "bg-primary",
                            IsRead = false
                        });
                    }
                }
            }
            // =========================================================================
            // 6. PATIENT NOTIFICATIONS (Only Patient's Registrations, Prescriptions & Blood Requests)
            // =========================================================================
            else if (role == SystemRoles.Patient)
            {
                // A. Camp Registrations for this Patient
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
                        Message = $"You are registered for '{reg.Camp?.Title}' ({reg.Camp?.District}). Date: {reg.Camp?.StartDate:dd MMM yyyy}.",
                        SenderName = reg.Camp?.Title ?? "MediCamp",
                        SenderRole = "Camp",
                        CampTitle = reg.Camp?.Title,
                        CampId = reg.CampId,
                        Timestamp = reg.RegisteredAt,
                        TimeAgo = FormatTimeAgo(reg.RegisteredAt),
                        Status = reg.Status,
                        CanApproveReject = false,
                        ActionUrl = Url.Action("History", "Patient") ?? "/Patient/History",
                        IconClass = "fa-solid fa-calendar-check",
                        BadgeColor = "bg-success",
                        IsRead = true
                    });
                }

                // B. Digital Prescriptions for this Patient
                var myPrescriptions = _dbContext.Prescriptions
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c!.TriageRecord)
                            .ThenInclude(t => t!.Camp)
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c!.Doctor)
                    .Where(p => p.Consultation != null && p.Consultation.TriageRecord != null && p.Consultation.TriageRecord.PatientId == user.Id)
                    .OrderByDescending(p => p.Id)
                    .Take(5)
                    .ToList();

                foreach (var p in myPrescriptions)
                {
                    var isDispensed = p.IsDispensed;
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"pat_rx_{p.Id}",
                        Type = "PrescriptionUpdate",
                        Title = isDispensed ? "Prescription Dispensed" : "Prescription Ready for Pickup",
                        Message = isDispensed
                            ? $"Your prescribed medicines for '{p.Consultation?.TriageRecord?.Camp?.Title ?? "Camp"}' have been dispensed."
                            : $"Dr. {p.Consultation?.Doctor?.FullName ?? "Doctor"} issued your prescription. Please collect medicines from the pharmacy counter.",
                        SenderName = p.Consultation?.Doctor?.FullName ?? "Medical Officer",
                        SenderRole = "Doctor",
                        CampTitle = p.Consultation?.TriageRecord?.Camp?.Title,
                        CampId = p.Consultation?.TriageRecord?.CampId,
                        Timestamp = p.DispensedAt ?? p.Consultation?.ConsultedAt ?? DateTime.UtcNow,
                        TimeAgo = FormatTimeAgo(p.DispensedAt ?? p.Consultation?.ConsultedAt ?? DateTime.UtcNow),
                        Status = isDispensed ? "Dispensed" : "Prescribed",
                        CanApproveReject = false,
                        ActionUrl = Url.Action("DigitalPrescription", "Patient", new { id = p.ConsultationId }) ?? "/Patient/History",
                        IconClass = "fa-solid fa-prescription",
                        BadgeColor = isDispensed ? "bg-success" : "bg-teal",
                        IsRead = isDispensed
                    });
                }

                // C. Blood Requests created by this Patient
                var myBloodRequests = _dbContext.BloodRequests
                    .Where(b => b.RequesterId == user.Id)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(4)
                    .ToList();

                foreach (var b in myBloodRequests)
                {
                    var isFulfilled = b.Status == "Fulfilled";
                    notifications.Add(new NotificationItemViewModel
                    {
                        Id = $"pat_blood_{b.Id}",
                        Type = "BloodRequestStatus",
                        Title = $"Blood Request Status: {b.Status}",
                        Message = $"Your request for {b.BloodGroup} ({b.UnitsRequired} units) at {b.HospitalName} is currently {b.Status.ToLower()}.",
                        SenderName = "Blood Network",
                        SenderRole = "System",
                        Timestamp = b.CreatedAt,
                        TimeAgo = FormatTimeAgo(b.CreatedAt),
                        Status = b.Status,
                        CanApproveReject = false,
                        ActionUrl = Url.Action("BloodDonation", "Patient") ?? "/Patient/BloodDonation",
                        IconClass = "fa-solid fa-droplet",
                        BadgeColor = isFulfilled ? "bg-success" : "bg-danger",
                        IsRead = isFulfilled
                    });
                }

                // D. Urgent Blood Match (Only if patient is registered as an available donor)
                var isDonor = _dbContext.BloodDonationProfiles.Any(d => d.UserId == user.Id && d.IsAvailableDonor);
                if (isDonor && !string.IsNullOrEmpty(user.BloodGroup))
                {
                    var matchBlood = _dbContext.BloodRequests
                        .Where(b => (b.Status == "Urgent" || b.Status == "Open") &&
                                    b.RequesterId != user.Id &&
                                    b.BloodGroup == user.BloodGroup &&
                                    (string.IsNullOrEmpty(user.District) || b.District == user.District))
                        .OrderByDescending(b => b.CreatedAt)
                        .Take(3)
                        .ToList();

                    foreach (var b in matchBlood)
                    {
                        notifications.Add(new NotificationItemViewModel
                        {
                            Id = $"pat_donor_match_{b.Id}",
                            Type = "DonorMatch",
                            Title = "Urgent Blood Need in Your Area",
                            Message = $"Patient urgently needs {b.BloodGroup} at {b.HospitalName}, {b.District}.",
                            SenderName = "Emergency Blood Alert",
                            SenderRole = "Patient",
                            Timestamp = b.CreatedAt,
                            TimeAgo = FormatTimeAgo(b.CreatedAt),
                            Status = "Urgent",
                            CanApproveReject = false,
                            ActionUrl = Url.Action("BloodDonation", "Patient") ?? "/Patient/BloodDonation",
                            IconClass = "fa-solid fa-heart-pulse",
                            BadgeColor = "bg-danger",
                            IsRead = false
                        });
                    }
                }
            }

            // Order strictly by timestamp descending
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
