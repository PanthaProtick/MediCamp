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
        // ==========================================
        // DOCTOR DASHBOARD & PATIENT QUEUE
        // ==========================================

        [HttpGet]
        public IActionResult Dashboard(int? activeCampId) => Queue(activeCampId);

        [HttpGet]
        public IActionResult Queue(int? activeCampId)
        {
            var doctorId = GetCurrentDoctorId();
            if (string.IsNullOrEmpty(doctorId))
                return RedirectToAction("Login", "Account");

            var today = DateTime.UtcNow.Date;

            // All ongoing camps active today or marked Ongoing
            var ongoingCamps = _dbContext.Camps
                .Where(c => (c.Status == "Ongoing" || (c.StartDate.Date <= today && today <= c.EndDate.Date)) &&
                            c.Status != "Rejected" && c.Status != "Cancelled" && c.Status != "Pending Admin Approval")
                .OrderByDescending(c => c.StartDate)
                .ToList();

            // All camps where this doctor has an approved staff request
            var approvedCamps = _dbContext.CampStaffRequests
                .Where(r => r.DoctorId == doctorId && r.Status == "Approved")
                .Select(r => r.Camp)
                .Where(c => c != null && (c.Status == "Ongoing" || (c.StartDate.Date <= today && today <= c.EndDate.Date)))
                .ToList()!;

            Camp? activeCamp = null;
            if (activeCampId.HasValue)
            {
                activeCamp = ongoingCamps.FirstOrDefault(c => c.Id == activeCampId.Value) 
                             ?? approvedCamps.FirstOrDefault(c => c.Id == activeCampId.Value)
                             ?? _dbContext.Camps.FirstOrDefault(c => c.Id == activeCampId.Value);
            }
            else if (ongoingCamps.Any())
            {
                activeCamp = ongoingCamps.First();
            }
            else if (approvedCamps.Any())
            {
                activeCamp = approvedCamps.First();
            }

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

            var doctorUser = _dbContext.Users.FirstOrDefault(u => u.Id == doctorId);
            var pendingRequestsCount = _mockDataService.GetRequestsForDoctor(doctorId).Count(r => r.Status == "Pending");
            var totalConsultationsCount = _dbContext.Consultations.Count(c => c.DoctorId == doctorId);
            var todayConsultationsCount = activeCamp != null
                ? _dbContext.Consultations.Count(c => c.TriageRecord != null && c.TriageRecord.CampId == activeCamp.Id && c.ConsultedAt.Date == today)
                : 0;

            var recentConsultations = activeCamp != null
                ? _dbContext.Consultations
                    .Include(c => c.TriageRecord)
                        .ThenInclude(t => t.Patient)
                    .Where(c => c.TriageRecord != null && c.TriageRecord.CampId == activeCamp.Id)
                    .OrderByDescending(c => c.ConsultedAt)
                    .Take(8)
                    .ToList()
                : new List<Consultation>();

            var model = new DoctorQueueViewModel
            {
                DoctorUser = doctorUser,
                OngoingCamps = ongoingCamps,
                ApprovedCamps = approvedCamps!,
                ActiveCamp = activeCamp,
                Queue = queue,
                PendingRequestsCount = pendingRequestsCount,
                TotalConsultationsCount = totalConsultationsCount,
                TodayConsultationsCount = todayConsultationsCount,
                RecentConsultations = recentConsultations
            };

            return View("Queue", model);
        }

        [HttpGet]
        public IActionResult ConsultByPatientId(int campId, string patientQuery)
        {
            var doctorId = GetCurrentDoctorId();
            if (string.IsNullOrEmpty(doctorId))
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(patientQuery))
            {
                TempData["ErrorMessage"] = "Please enter a Patient Unique ID to search.";
                return RedirectToAction(nameof(Queue), new { activeCampId = campId });
            }

            var cleanQuery = patientQuery.Trim().ToUpperInvariant();
            var patient = _dbContext.Users.FirstOrDefault(u => 
                (u.PatientUniqueId != null && u.PatientUniqueId.ToUpper() == cleanQuery) ||
                u.Id == patientQuery.Trim() ||
                u.PhoneNumber == patientQuery.Trim() ||
                u.NID == patientQuery.Trim() ||
                u.Email.ToLower() == patientQuery.Trim().ToLower());

            if (patient == null)
            {
                TempData["ErrorMessage"] = $"No patient found with Unique ID / Identifier \"{patientQuery}\". Please verify the 6-character ID.";
                return RedirectToAction(nameof(Queue), new { activeCampId = campId });
            }

            // Find existing triage in this camp
            var triage = _dbContext.TriageRecords
                .Include(t => t.Patient)
                .FirstOrDefault(t => t.CampId == campId && t.PatientId == patient.Id && !t.IsSeenByDoctor);

            if (triage != null)
            {
                return RedirectToAction(nameof(Consult), new { triageId = triage.Id });
            }

            // If already seen in this camp
            var alreadySeenTriage = _dbContext.TriageRecords
                .Include(t => t.Patient)
                .FirstOrDefault(t => t.CampId == campId && t.PatientId == patient.Id && t.IsSeenByDoctor);

            if (alreadySeenTriage != null)
            {
                TempData["ErrorMessage"] = $"Patient {patient.FullName} ({patient.PatientUniqueId}) has already completed consultation in this camp.";
                return RedirectToAction(nameof(Queue), new { activeCampId = campId });
            }

            // Create expedited triage intake record
            var nextToken = _dbContext.TriageRecords.Count(t => t.CampId == campId) + 1;
            var newTriage = new TriageRecord
            {
                CampId = campId,
                PatientId = patient.Id,
                VolunteerId = doctorId,
                BloodPressure = "120/80",
                TemperatureF = 98.6,
                WeightKg = 65.0,
                HeightCm = 165.0,
                BMI = 23.88,
                PresentingSymptoms = "Doctor Workspace Direct Intake",
                UrgencyLevel = "Normal",
                TokenNumber = nextToken,
                IsSeenByDoctor = false,
                RecordedAt = DateTime.UtcNow
            };

            _dbContext.TriageRecords.Add(newTriage);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Consult), new { triageId = newTriage.Id });
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

            // Camp inventory for prescription builder and stock cross-referencing
            var inventory = _dbContext.CampInventories
                .Include(ci => ci.MasterMedicine)
                .Where(ci => ci.CampId == triage.CampId)
                .ToList();

            var invMap = inventory.ToDictionary(ci => ci.MasterMedicineId, ci => ci.QuantityAllocated - ci.QuantityDispensed);

            var allMedicines = _dbContext.MasterMedicines
                .OrderBy(m => m.BrandName)
                .ToList();

            var medicineOptions = allMedicines.Select(m => new DoctorMedicineOption
            {
                MasterMedicineId = m.Id,
                BrandName = m.BrandName,
                GenericName = m.GenericName,
                DosageForm = m.DosageForm,
                Strength = m.Strength,
                Category = m.Category,
                InCampInventory = invMap.ContainsKey(m.Id),
                AvailableStock = invMap.TryGetValue(m.Id, out int stock) ? Math.Max(0, stock) : 0
            })
            .OrderByDescending(m => m.InCampInventory && m.AvailableStock > 0)
            .ThenByDescending(m => m.InCampInventory)
            .ThenBy(m => m.BrandName)
            .ToList();

            // Hospitals for referral dropdown
            var hospitals = _dbContext.Hospitals.OrderBy(h => h.Name).ToList();

            var model = new ConsultationWorkspaceViewModel
            {
                TriageRecord       = triage,
                Patient            = triage.Patient,
                PastVisits         = pastVisits,
                CampId             = triage.CampId,
                CampInventory      = inventory,
                MedicineOptions    = medicineOptions,
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
