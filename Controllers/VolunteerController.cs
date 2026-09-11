using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCamp.Models;
using MediCamp.Models.Domain;
using MediCamp.Models.ViewModels;
using MediCamp.Services;
using System.Security.Claims;

namespace MediCamp.Controllers
{
    [Authorize(Roles = "Volunteer")]
    public class VolunteerController : Controller
    {
        private readonly IMockDataService _mockDataService;
        private readonly MediCamp.Data.ApplicationDbContext _dbContext;

        public VolunteerController(IMockDataService mockDataService, MediCamp.Data.ApplicationDbContext dbContext)
        {
            _mockDataService = mockDataService;
            _dbContext = dbContext;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet]
        public IActionResult Dashboard(int? activeCampId, string? searchQuery)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var approvedCamps = _dbContext.CampVolunteerRequests
                .Where(r => r.VolunteerId == userId && r.Status == "Approved")
                .Select(r => r.Camp)
                .Where(c => c != null && c.Status == "Ongoing")
                .ToList();

            Camp? activeCamp = null;
            if (activeCampId.HasValue)
            {
                activeCamp = approvedCamps.FirstOrDefault(c => c.Id == activeCampId.Value);
            }
            else if (approvedCamps.Count == 1)
            {
                activeCamp = approvedCamps.First();
            }

            var searchResults = new List<ApplicationUser>();
            if (activeCamp != null && !string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                searchResults = _dbContext.Users
                    .Where(u => u.Role == SystemRoles.Patient && u.IsActive && 
                           (u.FullName.ToLower().Contains(lowerQuery) || 
                            u.PhoneNumber.Contains(lowerQuery)))
                    .Take(10)
                    .ToList();
            }

            var model = new VolunteerDashboardViewModel
            {
                ApprovedCamps = approvedCamps!,
                ActiveCamp = activeCamp,
                SearchResults = searchResults,
                SearchQuery = searchQuery
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Requests()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var allRequests = _mockDataService.GetRequestsForVolunteer(userId);

            var model = new VolunteerRequestsViewModel
            {
                PendingRequests = allRequests.Where(r => r.Status == "Pending").ToList(),
                RespondedRequests = allRequests.Where(r => r.Status != "Pending").ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RespondToRequest(int requestId, string status)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (status != "Approved" && status != "Denied")
            {
                TempData["ErrorMessage"] = "Invalid response status.";
                return RedirectToAction(nameof(Requests));
            }

            var result = _mockDataService.RespondToCampVolunteerRequest(requestId, userId, status);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Requests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApplyToCamp(int campId, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found.";
                return Redirect(returnUrl ?? Url.Action("Camps", "Home")!);
            }

            var existing = _dbContext.CampVolunteerRequests
                .FirstOrDefault(r => r.CampId == campId && r.VolunteerId == userId);

            if (existing != null)
            {
                TempData["ErrorMessage"] = $"You already have a request for \"{camp.Title}\" (Status: {existing.Status}).";
                return Redirect(returnUrl ?? Url.Action("Camps", "Home")!);
            }

            var newRequest = new CampVolunteerRequest
            {
                CampId = campId,
                VolunteerId = userId,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _dbContext.CampVolunteerRequests.Add(newRequest);
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"Your request to join \"{camp.Title}\" as Volunteer has been submitted to the host.";
            return Redirect(returnUrl ?? Url.Action("Camps", "Home")!);
        }
        [HttpGet]
        public IActionResult CreatePatient()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var model = new RegisterPatientViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePatient(RegisterPatientViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            // Remove password validation errors as we auto-generate it
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Password = "Patient@123";
            model.ConfirmPassword = "Patient@123";

            var result = _mockDataService.RegisterPatient(model);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Patient profile created successfully.";
                return RedirectToAction(nameof(CreatePatient));
            }
            
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        // ==========================================
        // TRIAGE
        // ==========================================
        [HttpGet]
        public IActionResult Triage(int campId, string patientId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var patient = _dbContext.Users.FirstOrDefault(u => u.Id == patientId && u.Role == SystemRoles.Patient);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var model = new TriageFormViewModel
            {
                CampId = campId,
                PatientId = patientId,
                Patient = patient
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitTriage(TriageFormViewModel model)
        {
            var volunteerId = GetCurrentUserId();
            if (volunteerId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                model.Patient = _dbContext.Users.FirstOrDefault(u => u.Id == model.PatientId);
                return View("Triage", model);
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == model.CampId);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var tokenNumber = _dbContext.TriageRecords.Count(t => t.CampId == model.CampId) + 1;

            var record = new TriageRecord
            {
                CampId = model.CampId,
                PatientId = model.PatientId,
                VolunteerId = volunteerId,
                BloodPressure = model.BloodPressure,
                TemperatureF = model.TemperatureF,
                WeightKg = model.WeightKg,
                HeightCm = model.HeightCm,
                BMI = model.BMI,
                PresentingSymptoms = model.PresentingSymptoms,
                UrgencyLevel = model.UrgencyLevel,
                TokenNumber = tokenNumber,
                IsSeenByDoctor = false
            };

            _dbContext.TriageRecords.Add(record);
            
            // Increment RegisteredPatientsCount
            camp.RegisteredPatientsCount += 1;

            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"Triage complete. Patient added to doctor queue with Token #{tokenNumber}.";
            return RedirectToAction(nameof(Dashboard), new { activeCampId = model.CampId });
        }

        // ==========================================
        // FOLLOW-UPS
        // ==========================================
        [HttpGet]
        public IActionResult FollowUps(int campId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!IsApprovedVolunteerForCamp(userId, campId))
            {
                return Forbid();
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId);
            if (camp == null)
            {
                return RedirectToAction(nameof(Dashboard));
            }

            // Get follow-ups for this camp
            var followUps = _dbContext.PatientFollowUps
                .Include(f => f.Patient)
                .Where(f => f.CampId == campId)
                .OrderBy(f => f.Status == "Pending" ? 0 : 1)
                .ThenBy(f => f.ScheduledDate)
                .ToList();

            // Also load patients seen in this camp for schedule modal
            var campPatients = _dbContext.TriageRecords
                .Include(t => t.Patient)
                .Where(t => t.CampId == campId && t.Patient != null)
                .Select(t => t.Patient!)
                .Distinct()
                .ToList();

            ViewBag.CampPatients = campPatients;

            var model = new VolunteerFollowUpViewModel
            {
                Camp = camp,
                FollowUps = followUps
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateFollowUpStatus(int followUpId, string status, string? notes, int campId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!IsApprovedVolunteerForCamp(userId, campId) || status is not ("Pending" or "Contacted" or "Resolved"))
            {
                return Forbid();
            }

            var followUp = _dbContext.PatientFollowUps.FirstOrDefault(f => f.Id == followUpId && f.CampId == campId);
            if (followUp != null)
            {
                followUp.Status = status;
                followUp.VolunteerNotes = string.IsNullOrWhiteSpace(notes) ? followUp.VolunteerNotes : notes.Trim();
                followUp.LastContactedAt = DateTime.UtcNow;
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Follow-up status updated.";
            }

            return RedirectToAction(nameof(FollowUps), new { campId = campId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ScheduleFollowUp(int campId, string patientId, string reason, DateTime scheduledDate)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (!IsApprovedVolunteerForCamp(userId, campId))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(patientId) || string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Please select a patient and provide a follow-up reason.";
                return RedirectToAction(nameof(FollowUps), new { campId });
            }

            var patientWasSeenAtCamp = _dbContext.TriageRecords.Any(t => t.CampId == campId && t.PatientId == patientId);
            if (!patientWasSeenAtCamp || scheduledDate.Date < DateTime.UtcNow.Date)
            {
                TempData["ErrorMessage"] = "Select a patient from this camp and choose today or a future date.";
                return RedirectToAction(nameof(FollowUps), new { campId });
            }

            var followUp = new PatientFollowUp
            {
                CampId        = campId,
                PatientId     = patientId,
                Reason        = reason,
                ScheduledDate = DateTime.SpecifyKind(scheduledDate, DateTimeKind.Utc),
                Status        = "Pending",
                CreatedAt     = DateTime.UtcNow
            };

            _dbContext.PatientFollowUps.Add(followUp);
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = "New patient follow-up scheduled successfully.";
            return RedirectToAction(nameof(FollowUps), new { campId });
        }

        private bool IsApprovedVolunteerForCamp(string userId, int campId)
        {
            return _dbContext.CampVolunteerRequests.Any(r =>
                r.CampId == campId && r.VolunteerId == userId && r.Status == "Approved");
        }
    }
}
