using MediCamp.Data;
using MediCamp.Models;
using MediCamp.Models.Domain;
using MediCamp.Models.ViewModels;
using MediCamp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace MediCamp.Controllers
{
    [Authorize(Roles = SystemRoles.Doctor)]
    public class DoctorController : Controller
    {
        private readonly IMockDataService _mockDataService;
        private readonly ApplicationDbContext _dbContext;

        public DoctorController(IMockDataService mockDataService, ApplicationDbContext dbContext)
        {
            _mockDataService = mockDataService;
            _dbContext = dbContext;
        }

        private string? GetCurrentDoctorId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId)) return userId;

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail)) return null;
            var user = _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == userEmail.ToLower());
            if (user != null) return user.Id;

            // Fallback to mock service
            var mockUser = _mockDataService.GetAllUsers().FirstOrDefault(u => u.Role == SystemRoles.Doctor);
            return mockUser?.Id;
        }

        // ==========================================
        // CAMP PARTICIPATION REQUESTS
        // ==========================================

        [HttpGet]
        public IActionResult Requests()
        {
            var doctorId = GetCurrentDoctorId();
            if (string.IsNullOrEmpty(doctorId))
                return RedirectToAction("Login", "Account");

            var allRequests = _mockDataService.GetRequestsForDoctor(doctorId);

            var model = new DoctorRequestsViewModel
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
            var doctorId = GetCurrentDoctorId();
            if (string.IsNullOrEmpty(doctorId))
                return RedirectToAction("Login", "Account");

            var result = _mockDataService.RespondToCampStaffRequest(requestId, doctorId, status);

            if (result.Success)
                TempData["SuccessMessage"] = result.Message;
            else
                TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Requests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApplyToCamp(int campId, string? returnUrl = null)
        {
            var doctorId = GetCurrentDoctorId();
            if (string.IsNullOrEmpty(doctorId)) return RedirectToAction("Login", "Account");

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found.";
                return Redirect(returnUrl ?? Url.Action("Camps", "Home")!);
            }

            var existing = _dbContext.CampStaffRequests
                .FirstOrDefault(r => r.CampId == campId && r.DoctorId == doctorId);

            if (existing != null)
            {
                TempData["ErrorMessage"] = $"You already have a request for \"{camp.Title}\" (Status: {existing.Status}).";
                return Redirect(returnUrl ?? Url.Action("Camps", "Home")!);
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

            TempData["SuccessMessage"] = $"Your request to join \"{camp.Title}\" as Doctor has been submitted to the host.";
            return Redirect(returnUrl ?? Url.Action("Camps", "Home")!);
        }

        // ==========================================
        // PATIENT QUEUE
        // ==========================================

        [HttpGet]
        public IActionResult Queue(int? activeCampId)
        {
            var doctorId = GetCurrentDoctorId();
            if (string.IsNullOrEmpty(doctorId))
                return RedirectToAction("Login", "Account");

            // All ongoing camps where this doctor has an approved request
            var approvedCamps = _dbContext.CampStaffRequests
                .Where(r => r.DoctorId == doctorId && r.Status == "Approved")
                .Select(r => r.Camp)
                .Where(c => c != null && c.Status == "Ongoing")
                .ToList()!;

            Camp? activeCamp = null;
            if (activeCampId.HasValue)
                activeCamp = approvedCamps.FirstOrDefault(c => c!.Id == activeCampId.Value);
            else if (approvedCamps.Count == 1)
                activeCamp = approvedCamps.First();

            var queue = new List<TriageRecord>();
            if (activeCamp != null)
            {
                queue = _dbContext.TriageRecords
                    .Include(t => t.Patient)
                    .Where(t => t.CampId == activeCamp.Id && !t.IsSeenByDoctor)
                    .OrderBy(t => t.UrgencyLevel == "Emergency" ? 0 :
                                  t.UrgencyLevel == "Urgent"    ? 1 : 2)
                    .ThenBy(t => t.TokenNumber)
                    .ToList();
            }

            var model = new DoctorQueueViewModel
            {
                ApprovedCamps = approvedCamps!,
                ActiveCamp = activeCamp,
                Queue = queue
            };

            return View(model);
        }

        // ==========================================
        // CONSULTATION WORKSPACE
        // ==========================================

        [HttpGet]
        public IActionResult Consult(int triageId)
        {
            var doctorId = GetCurrentDoctorId();
            if (string.IsNullOrEmpty(doctorId))
                return RedirectToAction("Login", "Account");

            var triage = _dbContext.TriageRecords
                .Include(t => t.Patient)
                .Include(t => t.Camp)
                .FirstOrDefault(t => t.Id == triageId);

            if (triage == null)
            {
                TempData["ErrorMessage"] = "Triage record not found.";
                return RedirectToAction(nameof(Queue));
            }

            // Guard: already seen
            if (triage.IsSeenByDoctor)
            {
                TempData["ErrorMessage"] = "This patient has already been consulted.";
                return RedirectToAction(nameof(Queue), new { activeCampId = triage.CampId });
            }

            // Build longitudinal history for the patient (all past completed visits)
            var pastTriageRecords = _dbContext.TriageRecords
                .Include(t => t.Camp)
                .Where(t => t.PatientId == triage.PatientId && t.IsSeenByDoctor && t.Id != triage.Id)
                .OrderByDescending(t => t.RecordedAt)
                .ToList();

            var pastVisits = new List<PatientVisitHistoryItem>();
            foreach (var pt in pastTriageRecords)
            {
                var consultation = _dbContext.Consultations
                    .FirstOrDefault(c => c.TriageRecordId == pt.Id);

                List<PrescriptionItem> rxItems = new();
                Referral? referral = null;
                if (consultation != null)
                {
                    var prescription = _dbContext.Prescriptions
                        .FirstOrDefault(p => p.ConsultationId == consultation.Id);
                    if (prescription != null)
                    {
                        rxItems = _dbContext.PrescriptionItems
                            .Include(pi => pi.MasterMedicine)
                            .Where(pi => pi.PrescriptionId == prescription.Id)
                            .ToList();
                    }
                    referral = _dbContext.Referrals
                        .FirstOrDefault(r => r.ConsultationId == consultation.Id);
                }

                pastVisits.Add(new PatientVisitHistoryItem
                {
                    Triage           = pt,
                    Consultation     = consultation,
                    PrescriptionItems = rxItems,
                    Referral         = referral,
                    CampTitle        = pt.Camp?.Title ?? "Unknown Camp"
                });
            }

            // Camp inventory for prescription builder
            var inventory = _dbContext.CampInventories
                .Include(ci => ci.MasterMedicine)
                .Where(ci => ci.CampId == triage.CampId)
                .ToList();

            // Hospitals for referral dropdown
            var hospitals = _dbContext.Hospitals.OrderBy(h => h.Name).ToList();

            var model = new ConsultationWorkspaceViewModel
            {
                TriageRecord      = triage,
                Patient           = triage.Patient,
                PastVisits        = pastVisits,
                CampId            = triage.CampId,
                CampInventory     = inventory,
                AvailableHospitals = hospitals
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitConsultation(
            int triageRecordId,
            int campId,
            string? diagnosis,
            string? clinicalNotes,
            string? advice,
            bool addReferral,
            string? referredHospital,
            string? referralReason,
            string referralUrgency,
            // Prescription items arrive as parallel arrays
            int[]? rxMedicineIds,
            string[]? rxDosages,
            int[]? rxDurations,
            string[]? rxInstructions,
            int[]? rxQuantities)
        {
            var doctorId = GetCurrentDoctorId();
            if (string.IsNullOrEmpty(doctorId))
                return RedirectToAction("Login", "Account");

            var triage = _dbContext.TriageRecords.FirstOrDefault(t => t.Id == triageRecordId);
            if (triage == null || triage.IsSeenByDoctor)
            {
                TempData["ErrorMessage"] = "Triage record not found or already processed.";
                return RedirectToAction(nameof(Queue), new { activeCampId = campId });
            }

            // 1. Save Consultation
            var consultation = new Consultation
            {
                TriageRecordId = triageRecordId,
                DoctorId       = doctorId,
                ConsultedAt    = DateTime.UtcNow,
                Diagnosis      = diagnosis?.Trim(),
                ClinicalNotes  = clinicalNotes?.Trim(),
                Advice         = advice?.Trim()
            };
            _dbContext.Consultations.Add(consultation);
            _dbContext.SaveChanges(); // Get consultation.Id

            // 2. Save Prescription + Items (if any medicines selected)
            bool hasPrescriptionItems = rxMedicineIds != null && rxMedicineIds.Length > 0;
            Prescription? prescription = null;
            if (hasPrescriptionItems)
            {
                prescription = new Prescription
                {
                    ConsultationId = consultation.Id,
                    IsDispensed    = false
                };
                _dbContext.Prescriptions.Add(prescription);
                _dbContext.SaveChanges(); // Get prescription.Id

                for (int i = 0; i < rxMedicineIds!.Length; i++)
                {
                    int qty = (rxQuantities != null && i < rxQuantities.Length) ? rxQuantities[i] : 1;
                    var item = new PrescriptionItem
                    {
                        PrescriptionId     = prescription.Id,
                        MasterMedicineId   = rxMedicineIds[i],
                        Dosage             = (rxDosages != null && i < rxDosages.Length) ? rxDosages[i] : "1+0+1",
                        DurationDays       = (rxDurations != null && i < rxDurations.Length) ? rxDurations[i] : 1,
                        Instructions       = (rxInstructions != null && i < rxInstructions.Length) ? rxInstructions[i]?.Trim() : null,
                        QuantityPrescribed = qty,
                        QuantityDispensed  = 0
                    };
                    _dbContext.PrescriptionItems.Add(item);
                }
                _dbContext.SaveChanges();
            }

            // 3. Save Referral (optional)
            if (addReferral && !string.IsNullOrWhiteSpace(referredHospital))
            {
                var referral = new Referral
                {
                    ConsultationId  = consultation.Id,
                    ReferredHospital = referredHospital.Trim(),
                    Reason          = referralReason?.Trim(),
                    Urgency         = string.IsNullOrWhiteSpace(referralUrgency) ? "Routine" : referralUrgency
                };
                _dbContext.Referrals.Add(referral);
            }

            // 4. Mark triage as seen and update camp counter
            triage.IsSeenByDoctor = true;
            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId);
            if (camp != null) camp.ServedPatientsCount += 1;

            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"Consultation saved successfully. {(hasPrescriptionItems ? "Prescription sent to pharmacy." : "No prescription issued.")}";
            return RedirectToAction(nameof(Queue), new { activeCampId = campId });
        }
    }
}
