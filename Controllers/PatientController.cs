using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCamp.Data;
using MediCamp.Models;
using MediCamp.Models.Domain;
using MediCamp.Models.ViewModels;
using MediCamp.Services;
using System.Security.Claims;

namespace MediCamp.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMockDataService _mockDataService;

        public PatientController(ApplicationDbContext dbContext, IMockDataService mockDataService)
        {
            _dbContext = dbContext;
            _mockDataService = mockDataService;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private ApplicationUser? GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(userEmail))
                {
                    return _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == userEmail.ToLower());
                }
                return null;
            }
            return _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        }

        [HttpGet]
        [Authorize(Roles = SystemRoles.Patient)]
        public IActionResult RegisterForCamp(int campId)
        {
            var patient = GetCurrentUser();
            if (patient == null) return RedirectToAction("Login", "Account");

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId &&
                (c.Status == "Scheduled" || c.Status == "Ongoing"));
            if (camp == null)
            {
                TempData["ErrorMessage"] = "This camp is not available for registration.";
                return RedirectToAction("Camps", "Home");
            }

            var alreadyRegistered = _dbContext.CampPatientRegistrations.Any(r =>
                r.CampId == campId && r.PatientId == patient.Id && r.Status == "Registered");

            return View(new CampPatientRegistrationViewModel
            {
                Camp = camp,
                Patient = patient,
                AlreadyRegistered = alreadyRegistered
            });
        }

        [HttpPost]
        [Authorize(Roles = SystemRoles.Patient)]
        [ValidateAntiForgeryToken]
        [ActionName("RegisterForCamp")]
        public IActionResult RegisterForCampSubmit(int campId)
        {
            var patient = GetCurrentUser();
            if (patient == null) return RedirectToAction("Login", "Account");

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId &&
                (c.Status == "Scheduled" || c.Status == "Ongoing"));
            if (camp == null)
            {
                TempData["ErrorMessage"] = "This camp is not available for registration.";
                return RedirectToAction("Camps", "Home");
            }

            var alreadyRegistered = _dbContext.CampPatientRegistrations.Any(r =>
                r.CampId == campId && r.PatientId == patient.Id && r.Status == "Registered");
            if (!alreadyRegistered)
            {
                _dbContext.CampPatientRegistrations.Add(new CampPatientRegistration
                {
                    CampId = campId,
                    PatientId = patient.Id,
                    Status = "Registered",
                    RegisteredAt = DateTime.UtcNow
                });
                camp.RegisteredPatientsCount++;
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = $"You are registered for {camp.Title}. Please arrive at the venue on time.";
            }
            else
            {
                TempData["SuccessMessage"] = "You are already registered for this camp.";
            }

            return RedirectToAction("Camps", "Home");
        }

        // ==========================================
        // 1. PATIENT MEDICAL HISTORY & PROFILE
        // ==========================================

        [HttpGet]
        public IActionResult History(string? patientId = null)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            // If a doctor or volunteer specifies patientId, allow viewing that patient's history
            string targetPatientId = currentUser.Id;
            if (!string.IsNullOrEmpty(patientId) && (User.IsInRole(SystemRoles.Doctor) || User.IsInRole(SystemRoles.Volunteer) || User.IsInRole(SystemRoles.Admin)))
            {
                targetPatientId = patientId;
            }

            var patient = _dbContext.Users.FirstOrDefault(u => u.Id == targetPatientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient record not found.";
                return RedirectToAction("Index", "Home");
            }

            var bloodProfile = _dbContext.BloodDonationProfiles.FirstOrDefault(b => b.UserId == targetPatientId);

            // Fetch all triage records for this patient
            var triages = _dbContext.TriageRecords
                .Include(t => t.Camp)
                .Where(t => t.PatientId == targetPatientId)
                .OrderByDescending(t => t.RecordedAt)
                .ToList();

            var visits = new List<PatientVisitTimelineItem>();

            foreach (var triage in triages)
            {
                var consultation = _dbContext.Consultations
                    .Include(c => c.Doctor)
                    .FirstOrDefault(c => c.TriageRecordId == triage.Id);

                Prescription? prescription = null;
                var rxItems = new List<PrescriptionItemDetail>();
                Referral? referral = null;

                if (consultation != null)
                {
                    prescription = _dbContext.Prescriptions
                        .FirstOrDefault(p => p.ConsultationId == consultation.Id);

                    if (prescription != null)
                    {
                        var rawItems = _dbContext.PrescriptionItems
                            .Include(pi => pi.MasterMedicine)
                            .Where(pi => pi.PrescriptionId == prescription.Id)
                            .ToList();

                        rxItems = rawItems.Select(ri => new PrescriptionItemDetail
                        {
                            MedicineName       = $"{ri.MasterMedicine?.BrandName} ({ri.MasterMedicine?.GenericName})",
                            Dosage             = ri.Dosage,
                            DurationDays       = ri.DurationDays,
                            Instructions       = ri.Instructions,
                            QuantityPrescribed = ri.QuantityPrescribed,
                            QuantityDispensed  = ri.QuantityDispensed
                        }).ToList();
                    }

                    referral = _dbContext.Referrals
                        .FirstOrDefault(r => r.ConsultationId == consultation.Id);
                }

                var followUp = _dbContext.PatientFollowUps
                    .FirstOrDefault(f => f.PatientId == targetPatientId && f.CampId == triage.CampId);

                visits.Add(new PatientVisitTimelineItem
                {
                    TriageId                = triage.Id,
                    CampTitle               = triage.Camp?.Title ?? "Medical Outreach Camp",
                    Location                = $"{triage.Camp?.Upazila}, {triage.Camp?.District}",
                    Venue                   = triage.Camp?.Venue ?? "",
                    VisitDate               = triage.RecordedAt,
                    TokenNumber             = triage.TokenNumber,
                    UrgencyLevel            = triage.UrgencyLevel,
                    BloodPressure           = triage.BloodPressure,
                    TemperatureF            = triage.TemperatureF,
                    WeightKg                = triage.WeightKg,
                    HeightCm                = triage.HeightCm,
                    BMI                     = triage.BMI,
                    PresentingSymptoms      = triage.PresentingSymptoms,
                    DoctorName              = consultation?.Doctor != null ? $"Dr. {consultation.Doctor.FullName}" : null,
                    Diagnosis               = consultation?.Diagnosis,
                    ClinicalNotes           = consultation?.ClinicalNotes,
                    Advice                  = consultation?.Advice,
                    ConsultedAt             = consultation?.ConsultedAt,
                    PrescriptionId          = prescription?.Id,
                    IsPrescriptionDispensed = prescription?.IsDispensed ?? false,
                    DispensedAt             = prescription?.DispensedAt,
                    PrescriptionItems       = rxItems,
                    ReferredHospital        = referral?.ReferredHospital,
                    ReferralReason          = referral?.Reason,
                    ReferralUrgency         = referral?.Urgency,
                    FollowUpReason          = followUp?.Reason,
                    FollowUpScheduledDate   = followUp?.ScheduledDate,
                    FollowUpStatus          = followUp?.Status
                });
            }

            var model = new PatientProfileHistoryViewModel
            {
                Patient      = patient,
                BloodProfile = bloodProfile,
                Visits       = visits
            };

            return View(model);
        }

        // ==========================================
        // 2. DIGITAL PRESCRIPTION SLIP
        // ==========================================

        [HttpGet]
        public IActionResult DigitalPrescription(int id)
        {
            var prescription = _dbContext.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.TriageRecord)
                        .ThenInclude(t => t!.Patient)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.TriageRecord)
                        .ThenInclude(t => t!.Camp)
                .FirstOrDefault(p => p.Id == id);

            if (prescription == null)
            {
                TempData["ErrorMessage"] = "Prescription not found.";
                return RedirectToAction(nameof(History));
            }

            var currentUserId = GetCurrentUserId();
            var patientId = prescription.Consultation?.TriageRecord?.PatientId;
            if (currentUserId == null || (patientId != currentUserId && !User.IsInRole(SystemRoles.Doctor) && !User.IsInRole(SystemRoles.Volunteer) && !User.IsInRole(SystemRoles.Admin)))
            {
                return Forbid();
            }

            var items = _dbContext.PrescriptionItems
                .Include(pi => pi.MasterMedicine)
                .Where(pi => pi.PrescriptionId == id)
                .ToList();

            var referral = prescription.Consultation != null
                ? _dbContext.Referrals.FirstOrDefault(r => r.ConsultationId == prescription.Consultation.Id)
                : null;

            var model = new DigitalPrescriptionViewModel
            {
                Prescription = prescription,
                Consultation = prescription.Consultation,
                TriageRecord = prescription.Consultation?.TriageRecord,
                Patient      = prescription.Consultation?.TriageRecord?.Patient,
                Doctor       = prescription.Consultation?.Doctor,
                Camp         = prescription.Consultation?.TriageRecord?.Camp,
                Items        = items,
                Referral     = referral
            };

            return View(model);
        }

        // ==========================================
        // 3. BLOOD DONATION HUB
        // ==========================================

        [HttpGet]
        [Authorize(Roles = SystemRoles.Patient)]
        public IActionResult BloodDonation(string? bloodGroup, string? district, string? upazila)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            // Ensure user has a BloodDonationProfile
            var myProfile = _dbContext.BloodDonationProfiles.FirstOrDefault(b => b.UserId == currentUser.Id);
            if (myProfile == null)
            {
                myProfile = new BloodDonationProfile
                {
                    UserId            = currentUser.Id,
                    BloodGroup        = currentUser.BloodGroup ?? "O+",
                    District          = currentUser.District ?? "Dhaka",
                    Upazila           = currentUser.Upazila ?? "Dhanmondi",
                    IsAvailableDonor  = true,
                    TotalDonationsCount = 0
                };
                _dbContext.BloodDonationProfiles.Add(myProfile);
                _dbContext.SaveChanges();
            }

            // Donor Search Query
            var donorsQuery = _dbContext.BloodDonationProfiles
                .Include(b => b.User)
                .Where(b => b.IsAvailableDonor && b.User != null && b.User.IsActive);

            if (!string.IsNullOrWhiteSpace(bloodGroup) && bloodGroup != "All")
            {
                donorsQuery = donorsQuery.Where(b => b.BloodGroup == bloodGroup);
            }

            if (!string.IsNullOrWhiteSpace(district) && district != "All")
            {
                donorsQuery = donorsQuery.Where(b => b.District.ToLower() == district.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(upazila) && upazila != "All")
            {
                donorsQuery = donorsQuery.Where(b => b.Upazila.ToLower() == upazila.ToLower());
            }

            var donorsList = donorsQuery
                .OrderByDescending(b => b.TotalDonationsCount)
                .Take(25)
                .Select(b => new DonorSearchResultItem
                {
                    UserId             = b.UserId,
                    FullName           = b.User!.FullName,
                    BloodGroup         = b.BloodGroup,
                    District           = b.District,
                    Upazila            = b.Upazila,
                    PhoneNumber        = b.User.PhoneNumber,
                    IsAvailableDonor   = b.IsAvailableDonor,
                    LastDonatedDate    = b.LastDonatedDate,
                    TotalDonationsCount = b.TotalDonationsCount
                })
                .ToList();

            // Open Blood Requests
            var openRequests = _dbContext.BloodRequests
                .Include(r => r.Requester)
                .Where(r => r.Status == "Open")
                .OrderByDescending(r => r.Urgency == "Critical" ? 2 : (r.Urgency == "Urgent" ? 1 : 0))
                .ThenByDescending(r => r.CreatedAt)
                .Take(20)
                .ToList();

            // User's own requests
            var myRequests = _dbContext.BloodRequests
                .Where(r => r.RequesterId == currentUser.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            // User's donation history log
            var myDonationLogs = _dbContext.BloodDonationLogs
                .Include(l => l.BloodRequest)
                .Where(l => l.DonorId == currentUser.Id)
                .OrderByDescending(l => l.DonatedDate)
                .ToList();

            // District & Upazila lists for filters
            var districts = _dbContext.Locations.Select(l => l.District).Distinct().OrderBy(d => d).ToList();
            if (!districts.Any())
            {
                districts = _dbContext.Camps.Select(c => c.District).Distinct().OrderBy(d => d).ToList();
            }

            var upazilas = _dbContext.Locations.Select(l => l.Upazila).Distinct().OrderBy(u => u).ToList();
            if (!upazilas.Any())
            {
                upazilas = _dbContext.Camps.Select(c => c.Upazila).Distinct().OrderBy(u => u).ToList();
            }

            var model = new BloodDonationHubViewModel
            {
                CurrentUser         = currentUser,
                MyProfile           = myProfile,
                SelectedBloodGroup  = bloodGroup,
                SelectedDistrict    = district,
                SelectedUpazila     = upazila,
                Donors              = donorsList,
                OpenRequests        = openRequests,
                MyRequests          = myRequests,
                MyDonationLogs      = myDonationLogs,
                Districts           = districts,
                Upazilas            = upazilas
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SystemRoles.Patient)]
        public IActionResult ToggleDonorStatus(bool isAvailableDonor, double? weightKg, DateTime? lastDonatedDate, string? district, string? upazila)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var profile = _dbContext.BloodDonationProfiles.FirstOrDefault(b => b.UserId == currentUser.Id);
            if (profile == null)
            {
                profile = new BloodDonationProfile { UserId = currentUser.Id };
                _dbContext.BloodDonationProfiles.Add(profile);
            }

            profile.IsAvailableDonor = isAvailableDonor;
            if (weightKg.HasValue) profile.WeightKg = weightKg.Value;
            if (lastDonatedDate.HasValue) profile.LastDonatedDate = DateTime.SpecifyKind(lastDonatedDate.Value, DateTimeKind.Utc);
            if (!string.IsNullOrWhiteSpace(district)) profile.District = district;
            if (!string.IsNullOrWhiteSpace(upazila)) profile.Upazila = upazila;
            if (!string.IsNullOrWhiteSpace(currentUser.BloodGroup)) profile.BloodGroup = currentUser.BloodGroup;

            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = isAvailableDonor
                ? "You are now listed as an active blood donor! Thank you for saving lives."
                : "Your donor status has been set to inactive.";

            return RedirectToAction(nameof(BloodDonation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SystemRoles.Patient)]
        public IActionResult CreateBloodRequest(CreateBloodRequestInput input)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields for the blood request.";
                return RedirectToAction(nameof(BloodDonation));
            }

            var request = new BloodRequest
            {
                RequesterId    = currentUser.Id,
                BloodGroup     = input.BloodGroup,
                UnitsRequired  = input.UnitsRequired,
                Urgency        = input.Urgency,
                HospitalName   = input.HospitalName,
                District       = input.District,
                Upazila        = input.Upazila ?? "",
                ContactNumber  = input.ContactNumber,
                Reason         = input.Reason,
                Status         = "Open",
                CreatedAt      = DateTime.UtcNow,
                NeededByDate   = input.NeededByDate.HasValue ? DateTime.SpecifyKind(input.NeededByDate.Value, DateTimeKind.Utc) : null
            };

            _dbContext.BloodRequests.Add(request);
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"Emergency blood request for {input.BloodGroup} ({input.UnitsRequired} Unit{(input.UnitsRequired > 1 ? "s" : "")}) posted successfully!";
            return RedirectToAction(nameof(BloodDonation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SystemRoles.Patient)]
        public IActionResult CancelBloodRequest(int requestId)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var request = _dbContext.BloodRequests.FirstOrDefault(r => r.Id == requestId && r.RequesterId == currentUser.Id);
            if (request != null)
            {
                request.Status = "Cancelled";
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Blood request cancelled.";
            }

            return RedirectToAction(nameof(BloodDonation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SystemRoles.Patient)]
        public IActionResult FulfillBloodRequest(int requestId)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var request = _dbContext.BloodRequests.FirstOrDefault(r => r.Id == requestId && r.RequesterId == currentUser.Id);
            if (request != null)
            {
                request.Status = "Fulfilled";
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Blood request marked as fulfilled!";
            }

            return RedirectToAction(nameof(BloodDonation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SystemRoles.Patient)]
        public IActionResult LogDonation(LogBloodDonationInput input)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid || input.DonatedDate.Date > DateTime.UtcNow.Date)
            {
                TempData["ErrorMessage"] = "Please provide a valid donation date and venue.";
                return RedirectToAction(nameof(BloodDonation));
            }

            var donationLog = new BloodDonationLog
            {
                DonorId          = currentUser.Id,
                DonatedDate      = DateTime.SpecifyKind(input.DonatedDate, DateTimeKind.Utc),
                VenueOrHospital  = input.VenueOrHospital,
                BloodRequestId   = input.BloodRequestId,
                Notes            = input.Notes
            };

            _dbContext.BloodDonationLogs.Add(donationLog);

            // Update user's profile
            var profile = _dbContext.BloodDonationProfiles.FirstOrDefault(b => b.UserId == currentUser.Id);
            if (profile != null)
            {
                profile.LastDonatedDate = DateTime.SpecifyKind(input.DonatedDate, DateTimeKind.Utc);
                profile.TotalDonationsCount += 1;
            }

            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Donation logged successfully! Your donor stats have been updated.";
            return RedirectToAction(nameof(BloodDonation));
        }
    }
}
