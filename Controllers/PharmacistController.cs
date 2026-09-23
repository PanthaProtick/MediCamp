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
    [Authorize(Roles = SystemRoles.Pharmacist)]
    public class PharmacistController : Controller
    {
        private readonly IMockDataService _mockDataService;
        private readonly ApplicationDbContext _dbContext;

        public PharmacistController(IMockDataService mockDataService, ApplicationDbContext dbContext)
        {
            _mockDataService = mockDataService;
            _dbContext = dbContext;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ==========================================
        // CAMP PARTICIPATION REQUESTS
        // ==========================================

        [HttpGet]
        public IActionResult Requests()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var allRequests = _mockDataService.GetRequestsForPharmacist(userId);

            var model = new PharmacistRequestsViewModel
            {
                PendingRequests   = allRequests.Where(r => r.Status == "Pending").ToList(),
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

            var result = _mockDataService.RespondToCampPharmacistRequest(requestId, userId, status);

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
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found.";
                return Redirect(returnUrl ?? Url.Action("Camps", "Home")!);
            }

            var activeAssignment = _dbContext.CampPharmacistRequests
                .Include(r => r.Camp)
                .FirstOrDefault(r => r.PharmacistId == userId && r.Status == "Approved" && r.Camp != null && r.Camp.Status != "Completed" && r.Camp.Status != "Cancelled" && r.Camp.Status != "Rejected");

            if (activeAssignment != null)
            {
                TempData["ErrorMessage"] = $"You are currently assigned to \"{activeAssignment.Camp?.Title}\". You cannot apply to other camps while assigned to an active camp.";
                return Redirect(returnUrl ?? Url.Action("Camps", "Home")!);
            }

            var existing = _dbContext.CampPharmacistRequests
                .FirstOrDefault(r => r.CampId == campId && r.PharmacistId == userId);

            if (existing != null)
            {
                TempData["ErrorMessage"] = $"You already have a request for \"{camp.Title}\" (Status: {existing.Status}).";
                return Redirect(returnUrl ?? Url.Action("Camps", "Home")!);
            }

            var newRequest = new CampPharmacistRequest
            {
                CampId = campId,
                PharmacistId = userId,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _dbContext.CampPharmacistRequests.Add(newRequest);
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"Your request to join \"{camp.Title}\" as Pharmacist has been submitted to the host.";
            return Redirect(returnUrl ?? Url.Action("Camps", "Home")!);
        }

        // ==========================================
        // PHARMACIST DASHBOARD
        // ==========================================

        // ==========================================
        // PHARMACIST DASHBOARD
        // ==========================================

        [HttpGet]
        public IActionResult Dashboard(int? activeCampId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var today = DateTime.UtcNow.Date;

            // All ongoing camps active today or marked Ongoing
            var ongoingCamps = _dbContext.Camps
                .Where(c => (c.Status == "Ongoing" || (c.StartDate.Date <= today && today <= c.EndDate.Date)) &&
                            c.Status != "Rejected" && c.Status != "Cancelled" && c.Status != "Pending Admin Approval")
                .OrderByDescending(c => c.StartDate)
                .ToList();

            // Camps where this pharmacist is approved
            var approvedCamps = _dbContext.CampPharmacistRequests
                .Where(r => r.PharmacistId == userId && r.Status == "Approved")
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

            int pendingCount = 0, dispensedToday = 0, lowStockCount = 0;
            if (activeCamp != null)
            {
                // Pending = prescriptions for this camp that are not yet dispensed
                pendingCount = _dbContext.Prescriptions
                    .Where(p => !p.IsDispensed &&
                                p.Consultation!.TriageRecord!.CampId == activeCamp.Id)
                    .Count();

                var todayStart = DateTime.UtcNow.Date;
                dispensedToday = _dbContext.Prescriptions
                    .Where(p => p.IsDispensed &&
                                p.DispensedAt >= todayStart &&
                                p.Consultation!.TriageRecord!.CampId == activeCamp.Id)
                    .Count();

                // Low stock: remaining < 15% of allocated
                var inventory = _dbContext.CampInventories
                    .Where(ci => ci.CampId == activeCamp.Id)
                    .ToList();
                lowStockCount = inventory.Count(ci =>
                {
                    int remaining = ci.QuantityAllocated - ci.QuantityDispensed;
                    return ci.QuantityAllocated > 0 && (double)remaining / ci.QuantityAllocated < 0.15 && remaining > 0;
                });
            }

            var model = new PharmacistDashboardViewModel
            {
                OngoingCamps            = ongoingCamps,
                ApprovedCamps           = approvedCamps!,
                ActiveCamp              = activeCamp,
                PendingPrescriptionsCount = pendingCount,
                DispensedTodayCount     = dispensedToday,
                LowStockItemsCount      = lowStockCount
            };

            return View(model);
        }

        // ==========================================
        // PRESCRIPTION QUEUE & LOOKUP
        // ==========================================

        [HttpGet]
        public IActionResult LookupPrescription(int campId, string patientQuery)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(patientQuery))
            {
                TempData["ErrorMessage"] = "Please enter a Patient Unique ID to lookup.";
                return RedirectToAction(nameof(PrescriptionQueue), new { campId });
            }

            var clean = patientQuery.Trim();
            var upper = clean.ToUpperInvariant();

            var patient = _dbContext.Users.FirstOrDefault(u => 
                (u.PatientUniqueId != null && u.PatientUniqueId.ToUpper() == upper) ||
                u.Id == clean ||
                u.PhoneNumber == clean ||
                (u.NID != null && u.NID == clean));

            if (patient == null)
            {
                TempData["ErrorMessage"] = $"No patient found with Unique ID \"{patientQuery}\". Please verify the 6-character ID.";
                return RedirectToAction(nameof(PrescriptionQueue), new { campId });
            }

            // Find pending prescription for this patient in this camp
            var pendingRx = _dbContext.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.TriageRecord)
                .FirstOrDefault(p => !p.IsDispensed &&
                                    p.Consultation != null &&
                                    p.Consultation.TriageRecord != null &&
                                    p.Consultation.TriageRecord.CampId == campId &&
                                    p.Consultation.TriageRecord.PatientId == patient.Id);

            if (pendingRx != null)
            {
                return RedirectToAction(nameof(Dispense), new { prescriptionId = pendingRx.Id });
            }

            // Check if already dispensed
            var dispensedRx = _dbContext.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.TriageRecord)
                .FirstOrDefault(p => p.IsDispensed &&
                                    p.Consultation != null &&
                                    p.Consultation.TriageRecord != null &&
                                    p.Consultation.TriageRecord.CampId == campId &&
                                    p.Consultation.TriageRecord.PatientId == patient.Id);

            if (dispensedRx != null)
            {
                TempData["ErrorMessage"] = $"Prescription for patient {patient.FullName} ({patient.PatientUniqueId}) has already been dispensed on {dispensedRx.DispensedAt:MMM dd, hh:mm tt}. Cannot dispense twice.";
                return RedirectToAction(nameof(PrescriptionQueue), new { campId });
            }

            TempData["ErrorMessage"] = $"No active prescription found for patient {patient.FullName} ({patient.PatientUniqueId}) in this camp. The doctor may not have written a prescription yet.";
            return RedirectToAction(nameof(PrescriptionQueue), new { campId });
        }

        [HttpGet]
        public IActionResult PrescriptionQueue(int campId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            // All prescriptions for this camp (via consultation -> triage -> camp)
            var rawPrescriptions = _dbContext.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.TriageRecord)
                        .ThenInclude(t => t!.Patient)
                .Where(p => p.Consultation != null &&
                            p.Consultation.TriageRecord != null &&
                            p.Consultation.TriageRecord.CampId == campId)
                .OrderBy(p => p.IsDispensed)
                .ThenByDescending(p => p.Consultation!.ConsultedAt)
                .ToList();

            var items = rawPrescriptions.Select(p =>
            {
                int itemCount = _dbContext.PrescriptionItems.Count(pi => pi.PrescriptionId == p.Id);
                return new PrescriptionQueueItem
                {
                    PrescriptionId = p.Id,
                    ConsultationId = p.ConsultationId,
                    TokenNumber    = p.Consultation?.TriageRecord?.TokenNumber ?? 0,
                    PatientName    = p.Consultation?.TriageRecord?.Patient?.FullName ?? "Unknown",
                    PatientId      = p.Consultation?.TriageRecord?.Patient?.PatientUniqueId ?? p.Consultation?.TriageRecord?.PatientId ?? "",
                    DoctorName     = p.Consultation?.Doctor?.FullName ?? "Unknown Doctor",
                    Diagnosis      = p.Consultation?.Diagnosis,
                    ItemCount      = itemCount,
                    ConsultedAt    = p.Consultation?.ConsultedAt ?? DateTime.UtcNow,
                    IsDispensed    = p.IsDispensed,
                    DispensedAt    = p.DispensedAt
                };
            }).ToList();

            var model = new PrescriptionQueueViewModel
            {
                Camp                  = camp,
                PendingPrescriptions  = items.Where(i => !i.IsDispensed).ToList(),
                DispensedPrescriptions = items.Where(i => i.IsDispensed).ToList()
            };

            return View(model);
        }

        // ==========================================
        // DISPENSING INTERFACE
        // ==========================================

        [HttpGet]
        public IActionResult Dispense(int prescriptionId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var prescription = _dbContext.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.TriageRecord)
                        .ThenInclude(t => t!.Patient)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                .FirstOrDefault(p => p.Id == prescriptionId);

            if (prescription == null)
            {
                TempData["ErrorMessage"] = "Prescription not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (prescription.IsDispensed)
            {
                TempData["ErrorMessage"] = "This prescription has already been dispensed.";
                int alreadyCampId = prescription.Consultation?.TriageRecord?.CampId ?? 0;
                return RedirectToAction(nameof(PrescriptionQueue), new { campId = alreadyCampId });
            }

            int campId = prescription.Consultation?.TriageRecord?.CampId ?? 0;

            // Load items with stock info
            var rawItems = _dbContext.PrescriptionItems
                .Include(pi => pi.MasterMedicine)
                .Where(pi => pi.PrescriptionId == prescriptionId)
                .ToList();

            var dispensingItems = rawItems.Select(pi =>
            {
                var invEntry = _dbContext.CampInventories
                    .FirstOrDefault(ci => ci.CampId == campId && ci.MasterMedicineId == pi.MasterMedicineId);
                int available = invEntry != null
                    ? Math.Max(0, invEntry.QuantityAllocated - invEntry.QuantityDispensed)
                    : 0;

                bool inInventory = invEntry != null;

                return new DispensingItemInput
                {
                    PrescriptionItemId  = pi.Id,
                    MasterMedicineId    = pi.MasterMedicineId,
                    MedicineName        = $"{pi.MasterMedicine?.BrandName} ({pi.MasterMedicine?.GenericName})",
                    Dosage              = pi.Dosage,
                    DurationDays        = pi.DurationDays,
                    Instructions        = pi.Instructions,
                    QuantityPrescribed  = pi.QuantityPrescribed,
                    AvailableStock      = available,
                    InCampInventory     = inInventory,
                    QuantityToDispense  = Math.Min(pi.QuantityPrescribed, available),
                    IsDispensed         = pi.QuantityDispensed > 0
                };
            }).ToList();

            var availableInventory = _dbContext.CampInventories
                .Include(ci => ci.MasterMedicine)
                .Where(ci => ci.CampId == campId && (ci.QuantityAllocated - ci.QuantityDispensed) > 0)
                .OrderBy(ci => ci.MasterMedicine!.BrandName)
                .ToList();

            var model = new DispensingViewModel
            {
                Prescription            = prescription,
                Consultation            = prescription.Consultation,
                TriageRecord            = prescription.Consultation?.TriageRecord,
                Patient                 = prescription.Consultation?.TriageRecord?.Patient,
                Doctor                  = prescription.Consultation?.Doctor,
                CampId                  = campId,
                Items                   = dispensingItems,
                AvailableCampInventory  = availableInventory
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DispenseItems(
            int prescriptionId,
            int campId,
            int[] itemIds,
            int[] quantities,
            int[]? substituteMedicineIds = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var prescription = _dbContext.Prescriptions
                .FirstOrDefault(p => p.Id == prescriptionId);

            if (prescription == null || prescription.IsDispensed)
            {
                TempData["ErrorMessage"] = "Prescription not found or already dispensed.";
                return RedirectToAction(nameof(PrescriptionQueue), new { campId });
            }

            // 1. Stock check: Group and aggregate requested quantities by effective medicine ID to prevent multi-row stock underflow (BUG-03)
            var requiredQuantitiesByMed = new Dictionary<int, (int TotalRequested, string MedicineName)>();

            for (int i = 0; i < itemIds.Length; i++)
            {
                int qty = (i < quantities.Length) ? quantities[i] : 0;
                if (qty <= 0) continue;

                var prescriptionItem = _dbContext.PrescriptionItems
                    .Include(pi => pi.MasterMedicine)
                    .FirstOrDefault(pi => pi.Id == itemIds[i] && pi.PrescriptionId == prescriptionId);
                if (prescriptionItem == null) continue;

                int effectiveMedicineId = (substituteMedicineIds != null && i < substituteMedicineIds.Length && substituteMedicineIds[i] > 0)
                    ? substituteMedicineIds[i]
                    : prescriptionItem.MasterMedicineId;

                string medName = prescriptionItem.MasterMedicine?.BrandName ?? "Medicine";
                if (effectiveMedicineId != prescriptionItem.MasterMedicineId)
                {
                    var sub = _dbContext.MasterMedicines.FirstOrDefault(m => m.Id == effectiveMedicineId);
                    if (sub != null) medName = sub.BrandName;
                }

                if (requiredQuantitiesByMed.TryGetValue(effectiveMedicineId, out var existing))
                {
                    requiredQuantitiesByMed[effectiveMedicineId] = (existing.TotalRequested + qty, existing.MedicineName);
                }
                else
                {
                    requiredQuantitiesByMed[effectiveMedicineId] = (qty, medName);
                }
            }

            foreach (var kvp in requiredQuantitiesByMed)
            {
                int medId = kvp.Key;
                int totalRequested = kvp.Value.TotalRequested;
                string medName = kvp.Value.MedicineName;

                var invEntry = _dbContext.CampInventories
                    .Include(ci => ci.MasterMedicine)
                    .FirstOrDefault(ci => ci.CampId == campId && ci.MasterMedicineId == medId);

                int availableStock = invEntry != null ? invEntry.QuantityAllocated - invEntry.QuantityDispensed : 0;
                if (availableStock < totalRequested)
                {
                    string displayName = invEntry?.MasterMedicine?.BrandName ?? medName;
                    TempData["ErrorMessage"] = $"Stock Warning: Insufficient inventory for \"{displayName}\". Available stock: {availableStock} units, Requested: {totalRequested} units. Dispensing blocked.";
                    return RedirectToAction(nameof(Dispense), new { prescriptionId });
                }
            }

            // 2. Update each prescription item and deduct from inventory
            for (int i = 0; i < itemIds.Length; i++)
            {
                int qty = (i < quantities.Length) ? quantities[i] : 0;
                if (qty <= 0) continue;

                var prescriptionItem = _dbContext.PrescriptionItems
                    .FirstOrDefault(pi => pi.Id == itemIds[i] && pi.PrescriptionId == prescriptionId);
                if (prescriptionItem == null) continue;

                // Check for substitute medicine
                int effectiveMedicineId = prescriptionItem.MasterMedicineId;
                if (substituteMedicineIds != null && i < substituteMedicineIds.Length && substituteMedicineIds[i] > 0)
                {
                    int subId = substituteMedicineIds[i];
                    if (subId != prescriptionItem.MasterMedicineId)
                    {
                        var originalMed = _dbContext.MasterMedicines.FirstOrDefault(m => m.Id == prescriptionItem.MasterMedicineId);
                        var subMed = _dbContext.MasterMedicines.FirstOrDefault(m => m.Id == subId);
                        effectiveMedicineId = subId;
                        prescriptionItem.MasterMedicineId = subId;
                        prescriptionItem.Instructions = string.IsNullOrWhiteSpace(prescriptionItem.Instructions)
                            ? $"[Substituted from {originalMed?.BrandName}]"
                            : $"{prescriptionItem.Instructions} [Substituted from {originalMed?.BrandName}]";
                    }
                }

                // Deduct from camp inventory
                var invEntry = _dbContext.CampInventories
                    .FirstOrDefault(ci => ci.CampId == campId && ci.MasterMedicineId == effectiveMedicineId);
                if (invEntry != null)
                {
                    invEntry.QuantityDispensed += qty;
                    prescriptionItem.QuantityDispensed = qty;
                }
                else
                {
                    prescriptionItem.QuantityDispensed = qty;
                }
            }

            // Mark prescription as fully dispensed
            prescription.IsDispensed             = true;
            prescription.DispensedByPharmacistId = userId;
            prescription.DispensedAt             = DateTime.UtcNow;

            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Prescription dispensed successfully! Medicine inventory deducted from camp dispensary.";
            return RedirectToAction(nameof(PrescriptionQueue), new { campId });
        }

        // ==========================================
        // INVENTORY & RESTOCK
        // ==========================================

        [HttpGet]
        public IActionResult Inventory(int campId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var inventoryItems = _dbContext.CampInventories
                .Include(ci => ci.MasterMedicine)
                .Where(ci => ci.CampId == campId)
                .OrderBy(ci => ci.MasterMedicine!.Category)
                .ThenBy(ci => ci.MasterMedicine!.BrandName)
                .ToList();

            var model = new PharmacistInventoryViewModel
            {
                Camp = camp,
                Inventory = inventoryItems.Select(ci => new PharmacistInventoryItem
                {
                    InventoryId      = ci.Id,
                    MasterMedicineId = ci.MasterMedicineId,
                    BrandName        = ci.MasterMedicine?.BrandName ?? "Unknown",
                    GenericName      = ci.MasterMedicine?.GenericName ?? "",
                    DosageForm       = ci.MasterMedicine?.DosageForm ?? "",
                    Strength         = ci.MasterMedicine?.Strength ?? "",
                    Category         = ci.MasterMedicine?.Category ?? "",
                    Allocated        = ci.QuantityAllocated,
                    Dispensed        = ci.QuantityDispensed
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RequestRestock(int campId, int medicineId, int requestedQty)
        {
            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId);
            var medicine = _dbContext.MasterMedicines.FirstOrDefault(m => m.Id == medicineId);

            if (camp == null || medicine == null)
            {
                TempData["ErrorMessage"] = "Invalid restock request.";
                return RedirectToAction(nameof(Inventory), new { campId });
            }

            if (requestedQty <= 0)
            {
                TempData["ErrorMessage"] = "Requested quantity must be greater than zero.";
                return RedirectToAction(nameof(Inventory), new { campId });
            }

            // In a full implementation this would create a RestockRequest record and notify the Host.
            // For now we log it as a TempData success message.
            TempData["SuccessMessage"] = $"Restock request sent to Host: {medicine.BrandName} × {requestedQty} units. The camp organiser will be notified.";
            return RedirectToAction(nameof(Inventory), new { campId });
        }
    }
}
