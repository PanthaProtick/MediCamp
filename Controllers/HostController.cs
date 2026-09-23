using MediCamp.Data;
using MediCamp.Models;
using MediCamp.Models.Domain;
using MediCamp.Models.ViewModels;
using MediCamp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MediCamp.Controllers
{
    [Authorize(Roles = SystemRoles.Host)]
    public class HostController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMockDataService _mockDataService;

        public HostController(ApplicationDbContext dbContext, IMockDataService mockDataService)
        {
            _dbContext = dbContext;
            _mockDataService = mockDataService;
        }

        private ApplicationUser? GetCurrentHostUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ApplicationUser? host = null;
            if (!string.IsNullOrEmpty(userEmail))
            {
                host = _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == userEmail.ToLower());
            }

            if (host == null && !string.IsNullOrEmpty(userId))
            {
                host = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            }

            if (host == null)
            {
                // Fallback to mock service if db context doesn't have session user yet
                var mockUser = _mockDataService.GetAllUsers().FirstOrDefault(u => u.Role == SystemRoles.Host);
                return mockUser;
            }

            return host;
        }

        // =========================================================================
        // 1. HOST DASHBOARD (/Host/Dashboard)
        // =========================================================================
        [HttpGet]
        public IActionResult Dashboard()
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch camps hosted by this user from EF Core PostgreSQL DB
            var myCamps = _dbContext.Camps
                .Where(c => c.HostId == currentHost.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            var model = new HostDashboardViewModel
            {
                HostUser = currentHost,
                MyCamps = myCamps
            };

            return View(model);
        }

        // =========================================================================
        // 2. CREATE NEW CAMP FORM (/Host/CreateCamp)
        // =========================================================================
        [HttpGet]
        public IActionResult CreateCamp()
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Access Control Enforcement: Approved Hosts ONLY
            if (currentHost.HostApprovalStatus != "Approved")
            {
                TempData["ErrorMessage"] = "Access Denied: Your Host organization registration must be reviewed and approved by System Administrators before creating camps.";
                return RedirectToAction(nameof(Dashboard));
            }

            var model = new CreateCampViewModel();
            PopulateDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCamp(CreateCampViewModel model)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Access Control Enforcement: Approved Hosts ONLY
            if (currentHost.HostApprovalStatus != "Approved")
            {
                TempData["ErrorMessage"] = "Access Denied: Your Host organization registration must be approved before creating camps.";
                return RedirectToAction(nameof(Dashboard));
            }

            // Validation: End Date >= Start Date
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Camp End Date must be on or after the Start Date.");
            }

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return View(model);
            }

            // Create Domain Entity for EF Core PostgreSQL Persistence
            var newCamp = new Camp
            {
                Title = model.Title.Trim(),
                CampType = model.CampType,
                District = model.District,
                Upazila = model.Upazila,
                Venue = model.Venue.Trim(),
                StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc),
                ExpectedPatients = model.ExpectedPatients,
                RegisteredPatientsCount = 0,
                ServedPatientsCount = 0,
                TotalBudget = model.TotalBudget,
                UtilizedBudget = 0,
                Status = "Pending Admin Approval", // Status requirement: Pending Admin Approval
                HostId = currentHost.Id,
                Description = model.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            // Persist to PostgreSQL database
            _dbContext.Camps.Add(newCamp);
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Medical Camp application submitted successfully! Pending Admin approval.";
            return RedirectToAction(nameof(CampSubmitted), new { id = newCamp.Id });
        }

        // =========================================================================
        // 3. CASCADING AJAX DROPDOWN API (/Host/GetUpazilas)
        // =========================================================================
        [HttpGet]
        public IActionResult GetUpazilas(string district)
        {
            if (string.IsNullOrWhiteSpace(district))
            {
                return Json(new List<string>());
            }

            var upazilas = _dbContext.Locations
                .Where(l => l.District.ToLower() == district.Trim().ToLower())
                .Select(l => l.Upazila)
                .Distinct()
                .OrderBy(u => u)
                .ToList();

            if (!upazilas.Any())
            {
                // Fallback default upazilas if specific district hasn't been seeded yet
                upazilas = new List<string> { $"{district} Sadar", "Central Ward 1", "Upazila Health Complex" };
            }

            return Json(upazilas);
        }

        // =========================================================================
        // 4. CONFIRMATION VIEW (/Host/CampSubmitted/{id})
        // =========================================================================
        [HttpGet]
        public IActionResult CampSubmitted(int id)
        {
            var camp = _dbContext.Camps
                .Include(c => c.Host)
                .FirstOrDefault(c => c.Id == id);

            if (camp == null)
            {
                return RedirectToAction(nameof(Dashboard));
            }

            return View(camp);
        }

        // =========================================================================
        // 5. MY CAMPS LIST VIEW (/Host/MyCamps)
        // =========================================================================
        [HttpGet]
        public IActionResult MyCamps(string? status)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var campsQuery = _dbContext.Camps
                .Where(c => c.HostId == currentHost.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                campsQuery = campsQuery.Where(c => c.Status == status);
            }

            ViewBag.SelectedStatus = status ?? "All";
            ViewBag.HostApprovalStatus = currentHost.HostApprovalStatus;

            var myCamps = campsQuery.OrderByDescending(c => c.CreatedAt).ToList();
            return View(myCamps);
        }

        // =========================================================================
        // 5b. CAMP DETAILS ROUTING (/Host/Details/{id})
        // =========================================================================
        [HttpGet]
        public IActionResult Details(int id)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found.";
                return RedirectToAction(nameof(MyCamps));
            }

            return RedirectToAction(nameof(ManageStaff), new { id = id });
        }


        // Helper method to populate dynamic location and camp type dropdown lists
        private void PopulateDropdowns(CreateCampViewModel model)
        {
            model.AvailableCampTypes = CampTypes.AllTypes;

            // Fetch dynamic districts from Locations table in EF Core
            var districts = _dbContext.Locations
                .Select(l => l.District)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            if (!districts.Any())
            {
                districts = new List<string> { "Kurigram", "Sunamganj", "Bandarban", "Dhaka", "Chittagong", "Sylhet", "Rangpur", "Khulna", "Barisal", "Rajshahi", "Mymensingh" };
            }

            model.AvailableDistricts = districts;

            if (string.IsNullOrWhiteSpace(model.District))
            {
                model.District = districts.FirstOrDefault() ?? "Kurigram";
            }

            // Fetch dynamic upazilas for selected district
            var upazilas = _dbContext.Locations
                .Where(l => l.District.ToLower() == model.District.ToLower())
                .Select(l => l.Upazila)
                .Distinct()
                .OrderBy(u => u)
                .ToList();

            if (!upazilas.Any())
            {
                upazilas = new List<string> { $"{model.District} Sadar", "Chilmari", "Tahirpur", "Ruma" };
            }

            model.AvailableUpazilas = upazilas;

            if (string.IsNullOrWhiteSpace(model.Upazila))
            {
                model.Upazila = upazilas.FirstOrDefault() ?? string.Empty;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResubmitCamp(int id)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null || currentHost.HostApprovalStatus != "Approved")
            {
                return RedirectToAction(nameof(Dashboard));
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == id && c.HostId == currentHost.Id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found or you do not have permission.";
                return RedirectToAction(nameof(MyCamps));
            }

            if (camp.Status != "Rejected")
            {
                TempData["ErrorMessage"] = "Only rejected camps can be resubmitted.";
                return RedirectToAction(nameof(MyCamps));
            }

            camp.Status = "Pending Admin Approval";
            camp.CampRejectionReason = null;
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"Camp '{camp.Title}' has been resubmitted for Admin Approval.";
            return RedirectToAction(nameof(MyCamps), new { status = "Pending Admin Approval" });
        }

        // =========================================================================
        // 6. MANAGE CAMP STAFF (/Host/ManageStaff/{id})
        // =========================================================================
        [HttpGet]
        public IActionResult ManageStaff(int id, string? doctorSearch, string? volunteerSearch, string? pharmacistSearch, int doctorPage = 1, int volunteerPage = 1, int pharmacistPage = 1)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == id && c.HostId == currentHost.Id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found or access denied.";
                return RedirectToAction(nameof(MyCamps));
            }

            var doctorsQuery = _dbContext.Users.Where(u => u.Role == SystemRoles.Doctor && u.IsActive);
            if (!string.IsNullOrWhiteSpace(doctorSearch))
            {
                var lowerSearch = doctorSearch.ToLower();
                doctorsQuery = doctorsQuery.Where(u => u.FullName.ToLower().Contains(lowerSearch) || 
                                                       (u.MedicalSpecialization != null && u.MedicalSpecialization.ToLower().Contains(lowerSearch)));
            }

            var volunteersQuery = _dbContext.Users.Where(u => u.Role == SystemRoles.Volunteer && u.IsActive);
            if (!string.IsNullOrWhiteSpace(volunteerSearch))
            {
                var lowerSearch = volunteerSearch.ToLower();
                volunteersQuery = volunteersQuery.Where(u => u.FullName.ToLower().Contains(lowerSearch) || 
                                                             (u.District != null && u.District.ToLower().Contains(lowerSearch)));
            }

            var pharmacistsQuery = _dbContext.Users.Where(u => u.Role == "Pharmacist" && u.IsActive);
            if (!string.IsNullOrWhiteSpace(pharmacistSearch))
            {
                var lowerSearch = pharmacistSearch.ToLower();
                pharmacistsQuery = pharmacistsQuery.Where(u => u.FullName.ToLower().Contains(lowerSearch) || 
                                                               (u.District != null && u.District.ToLower().Contains(lowerSearch)));
            }
            
            int pageSize = 6; // Changed to 6 so it displays well in a 2-column or 3-column grid

            var totalDoctors = doctorsQuery.Count();
            var totalVolunteers = volunteersQuery.Count();
            var totalPharmacists = pharmacistsQuery.Count();

            var model = new HostManageStaffViewModel
            {
                Camp = camp,
                CurrentRequests = _mockDataService.GetRequestsForCamp(id),
                AvailableDoctors = doctorsQuery.Skip((doctorPage - 1) * pageSize).Take(pageSize).ToList(),
                CurrentVolunteerRequests = _mockDataService.GetVolunteerRequestsForCamp(id),
                AvailableVolunteers = volunteersQuery.Skip((volunteerPage - 1) * pageSize).Take(pageSize).ToList(),
                CurrentPharmacistRequests = _mockDataService.GetPharmacistRequestsForCamp(id),
                AvailablePharmacists = pharmacistsQuery.Skip((pharmacistPage - 1) * pageSize).Take(pageSize).ToList(),

                DoctorAssignedCamps = _mockDataService.GetActiveDoctorAssignments(),
                VolunteerAssignedCamps = _mockDataService.GetActiveVolunteerAssignments(),
                PharmacistAssignedCamps = _mockDataService.GetActivePharmacistAssignments(),
                
                CurrentDoctorPage = doctorPage,
                TotalDoctorPages = (int)Math.Ceiling(totalDoctors / (double)pageSize),
                DoctorPageSize = pageSize,

                CurrentVolunteerPage = volunteerPage,
                TotalVolunteerPages = (int)Math.Ceiling(totalVolunteers / (double)pageSize),
                VolunteerPageSize = pageSize,

                CurrentPharmacistPage = pharmacistPage,
                TotalPharmacistPages = (int)Math.Ceiling(totalPharmacists / (double)pageSize),
                PharmacistPageSize = pageSize
            };

            ViewBag.DoctorSearch = doctorSearch;
            ViewBag.VolunteerSearch = volunteerSearch;
            ViewBag.PharmacistSearch = pharmacistSearch;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendStaffRequest(int campId, string doctorId)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId && c.HostId == currentHost.Id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found or access denied.";
                return RedirectToAction(nameof(MyCamps));
            }

            var result = _mockDataService.SendCampStaffRequest(campId, doctorId);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(ManageStaff), new { id = campId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendVolunteerRequest(int campId, string volunteerId)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId && c.HostId == currentHost.Id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found or access denied.";
                return RedirectToAction(nameof(MyCamps));
            }

            var result = _mockDataService.SendCampVolunteerRequest(campId, volunteerId);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(ManageStaff), new { id = campId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendPharmacistRequest(int campId, string pharmacistId)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId && c.HostId == currentHost.Id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found or access denied.";
                return RedirectToAction(nameof(MyCamps));
            }

            var result = _mockDataService.SendCampPharmacistRequest(campId, pharmacistId);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(ManageStaff), new { id = campId });
        }
        // =========================================================================
        // 7. MANAGE CAMP INVENTORY (/Host/ManageInventory/{id})
        // =========================================================================
        [HttpGet]
        public IActionResult ManageInventory(int id)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == id && c.HostId == currentHost.Id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found or access denied.";
                return RedirectToAction(nameof(MyCamps));
            }

            var currentInventory = _dbContext.CampInventories
                .Include(i => i.MasterMedicine)
                .Where(i => i.CampId == id)
                .OrderBy(i => i.MasterMedicine.Category)
                .ThenBy(i => i.MasterMedicine.BrandName)
                .ToList();

            var availableMedicines = _dbContext.MasterMedicines
                .OrderBy(m => m.Category)
                .ThenBy(m => m.BrandName)
                .ToList();

            var model = new HostManageInventoryViewModel
            {
                Camp = camp,
                CurrentInventory = currentInventory,
                AvailableMedicines = availableMedicines
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AllocateMedicine(int campId, HostManageInventoryViewModel inputModel)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId && c.HostId == currentHost.Id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found or access denied.";
                return RedirectToAction(nameof(MyCamps));
            }

            if (inputModel.SelectedMedicineId <= 0 || inputModel.QuantityToAllocate <= 0)
            {
                TempData["ErrorMessage"] = "Invalid medicine selection or quantity.";
                return RedirectToAction(nameof(ManageInventory), new { id = campId });
            }

            var existingRecord = _dbContext.CampInventories
                .FirstOrDefault(i => i.CampId == campId && i.MasterMedicineId == inputModel.SelectedMedicineId);

            if (existingRecord != null)
            {
                existingRecord.QuantityAllocated += inputModel.QuantityToAllocate;
            }
            else
            {
                _dbContext.CampInventories.Add(new CampInventory
                {
                    CampId = campId,
                    MasterMedicineId = inputModel.SelectedMedicineId,
                    QuantityAllocated = inputModel.QuantityToAllocate,
                    QuantityDispensed = 0
                });
            }

            _dbContext.SaveChanges();
            TempData["SuccessMessage"] = "Medicine successfully allocated to camp inventory.";
            
            return RedirectToAction(nameof(ManageInventory), new { id = campId });
        }

        // =========================================================================
        // 8. MONITOR CAMP FINANCIALS (/Host/MonitorCamp/{id})
        // =========================================================================
        [HttpGet]
        public IActionResult MonitorCamp(int id)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == id && c.HostId == currentHost.Id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found or access denied.";
                return RedirectToAction(nameof(MyCamps));
            }

            var model = new HostMonitorCampViewModel
            {
                Camp = camp
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateBudget(int campId, HostMonitorCampViewModel inputModel)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == campId && c.HostId == currentHost.Id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found or access denied.";
                return RedirectToAction(nameof(MyCamps));
            }

            if (inputModel.AdditionalExpense <= 0)
            {
                TempData["ErrorMessage"] = "Expense amount must be greater than zero.";
                return RedirectToAction(nameof(MonitorCamp), new { id = campId });
            }

            if (camp.UtilizedBudget + inputModel.AdditionalExpense > camp.TotalBudget)
            {
                TempData["ErrorMessage"] = "Error: Adding this expense would exceed the Total Allocated Budget.";
                return RedirectToAction(nameof(MonitorCamp), new { id = campId });
            }

            camp.UtilizedBudget += inputModel.AdditionalExpense;
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"Successfully logged expense of ৳{inputModel.AdditionalExpense:N2}.";
            return RedirectToAction(nameof(MonitorCamp), new { id = campId });
        }

        // =========================================================================
        // 9. HOST REPORTS & ANALYTICS (/Host/Reports)
        // =========================================================================
        [HttpGet]
        public IActionResult Reports(string tab = "financial", int? campId = null)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myCamps = _dbContext.Camps
                .Where(c => c.HostId == currentHost.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            var selectedCamp = campId.HasValue && campId.Value > 0
                ? myCamps.FirstOrDefault(c => c.Id == campId.Value)
                : null;

            var campIds = selectedCamp != null 
                ? new List<int> { selectedCamp.Id } 
                : myCamps.Select(c => c.Id).ToList();

            var model = new HostReportsViewModel
            {
                ActiveTab = string.IsNullOrWhiteSpace(tab) ? "financial" : tab.ToLowerInvariant(),
                SelectedCampId = campId,
                HostCamps = myCamps,
                SelectedCamp = selectedCamp
            };

            // -------------------------------------------------------------
            // A. Financial Report Data Scoped to Host Camps
            // -------------------------------------------------------------
            decimal totalBudget = selectedCamp != null 
                ? selectedCamp.TotalBudget 
                : myCamps.Sum(c => c.TotalBudget);

            var expensesQuery = _dbContext.CampExpenses
                .Include(e => e.Camp)
                .Where(e => campIds.Contains(e.CampId))
                .OrderByDescending(e => e.ExpenseDate)
                .ToList();

            decimal totalSpent = expensesQuery.Any() 
                ? expensesQuery.Sum(e => e.Amount)
                : (selectedCamp != null ? selectedCamp.UtilizedBudget : myCamps.Sum(c => c.UtilizedBudget));

            int totalPatientsServed = selectedCamp != null 
                ? selectedCamp.ServedPatientsCount 
                : myCamps.Sum(c => c.ServedPatientsCount);

            if (totalPatientsServed == 0)
            {
                totalPatientsServed = _dbContext.TriageRecords.Count(t => campIds.Contains(t.CampId) && t.IsSeenByDoctor);
            }

            // Standard Categories
            var categories = new[]
            {
                ("Medicines & Medical Supplies", 0.40m, "text-success"),
                ("Doctor & Staff Honorarium", 0.25m, "text-primary"),
                ("Logistics & Transportation", 0.15m, "text-warning"),
                ("Venue, Tents & Facilities", 0.10m, "text-info"),
                ("Refreshments & Volunteers", 0.05m, "text-secondary"),
                ("Diagnostic Tools & Equipment", 0.05m, "text-danger")
            };

            var categoryBreakdown = new List<CategoryExpenseItem>();
            foreach (var cat in categories)
            {
                decimal catSpent = expensesQuery.Where(e => e.Category == cat.Item1).Sum(e => e.Amount);
                decimal catBudget = totalBudget * cat.Item2;
                double pctOfTotal = totalSpent > 0 ? (double)(catSpent / totalSpent) * 100 : 0;

                categoryBreakdown.Add(new CategoryExpenseItem
                {
                    Category = cat.Item1,
                    AllocatedBudget = catBudget,
                    ActualSpent = catSpent,
                    PercentageOfTotalSpent = Math.Round(pctOfTotal, 1),
                    ColorClass = cat.Item3
                });
            }

            model.FinancialReport = new HostFinancialReportViewModel
            {
                TotalBudget = totalBudget,
                TotalSpent = totalSpent,
                TotalPatientsServed = totalPatientsServed,
                CategoryBreakdown = categoryBreakdown,
                ExpenseLogs = expensesQuery,
                NewExpenseInput = new CampExpenseInputModel { CampId = selectedCamp?.Id ?? (myCamps.FirstOrDefault()?.Id ?? 0) }
            };

            // -------------------------------------------------------------
            // B. Staff Performance Report Data Scoped to Host Camps
            // -------------------------------------------------------------
            var doctorUsers = _dbContext.Users.Where(u => u.Role == SystemRoles.Doctor).ToList();
            var doctorStats = new List<DoctorPerformanceItem>();

            foreach (var doc in doctorUsers)
            {
                var consultations = _dbContext.Consultations
                    .Include(c => c.TriageRecord)
                    .Where(c => c.DoctorId == doc.Id && c.TriageRecord != null && campIds.Contains(c.TriageRecord.CampId))
                    .ToList();

                var consultIds = consultations.Select(c => c.Id).ToList();
                var prescriptionsCount = _dbContext.Prescriptions.Count(p => consultIds.Contains(p.ConsultationId));
                var referralsCount = _dbContext.Referrals.Count(r => consultIds.Contains(r.ConsultationId));

                int campsAttended = consultations.Select(c => c.TriageRecord!.CampId).Distinct().Count();
                if (campsAttended == 0)
                {
                    campsAttended = _dbContext.CampStaffRequests.Count(r => r.DoctorId == doc.Id && campIds.Contains(r.CampId) && r.Status == "Approved");
                }

                if (consultations.Any() || campsAttended > 0)
                {
                    doctorStats.Add(new DoctorPerformanceItem
                    {
                        DoctorId = doc.Id,
                        DoctorName = doc.FullName,
                        Specialization = doc.MedicalSpecialization ?? "General Practitioner",
                        BMDCRegNo = doc.BMDCRegNo ?? "Verified",
                        CampsAttended = campsAttended,
                        PatientsConsulted = consultations.Count,
                        PrescriptionsIssued = prescriptionsCount,
                        ReferralsMade = referralsCount
                    });
                }
            }

            var volunteerUsers = _dbContext.Users.Where(u => u.Role == SystemRoles.Volunteer).ToList();
            var volunteerStats = new List<VolunteerPerformanceItem>();

            foreach (var vol in volunteerUsers)
            {
                int triageCount = _dbContext.TriageRecords.Count(t => t.VolunteerId == vol.Id && campIds.Contains(t.CampId));
                int followUpsAssigned = _dbContext.PatientFollowUps.Count(f => campIds.Contains(f.CampId));
                int followUpsCompleted = _dbContext.PatientFollowUps.Count(f => campIds.Contains(f.CampId) && (f.Status == "Resolved" || f.Status == "Contacted"));

                if (triageCount > 0 || followUpsAssigned > 0)
                {
                    volunteerStats.Add(new VolunteerPerformanceItem
                    {
                        VolunteerId = vol.Id,
                        VolunteerName = vol.FullName,
                        District = vol.District ?? "Field Unit",
                        TriageRecordsLogged = triageCount,
                        FollowUpsAssigned = followUpsAssigned,
                        FollowUpsCompleted = followUpsCompleted
                    });
                }
            }

            var pharmacistUsers = _dbContext.Users.Where(u => u.Role == SystemRoles.Pharmacist).ToList();
            var pharmacistStats = new List<PharmacistPerformanceItem>();

            foreach (var pharma in pharmacistUsers)
            {
                var dispensedPrescriptions = _dbContext.Prescriptions
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c!.TriageRecord)
                    .Where(p => p.IsDispensed && p.DispensedByPharmacistId == pharma.Id && p.Consultation != null && p.Consultation.TriageRecord != null && campIds.Contains(p.Consultation.TriageRecord.CampId))
                    .ToList();

                var pIds = dispensedPrescriptions.Select(p => p.Id).ToList();
                int totalUnits = _dbContext.PrescriptionItems.Where(pi => pIds.Contains(pi.PrescriptionId)).Sum(pi => pi.QuantityDispensed);

                if (dispensedPrescriptions.Any() || totalUnits > 0)
                {
                    pharmacistStats.Add(new PharmacistPerformanceItem
                    {
                        PharmacistId = pharma.Id,
                        PharmacistName = pharma.FullName,
                        PrescriptionsDispensed = dispensedPrescriptions.Count,
                        TotalMedicineUnitsDispensed = totalUnits > 0 ? totalUnits : dispensedPrescriptions.Count * 14
                    });
                }
            }

            model.StaffReport = new HostStaffPerformanceReportViewModel
            {
                DoctorStats = doctorStats.OrderByDescending(d => d.PatientsConsulted).ToList(),
                VolunteerStats = volunteerStats.OrderByDescending(v => v.TriageRecordsLogged).ToList(),
                PharmacistStats = pharmacistStats.OrderByDescending(p => p.PrescriptionsDispensed).ToList()
            };

            // -------------------------------------------------------------
            // C. Patient Demographics Report Data Scoped to Host Camps
            // -------------------------------------------------------------
            var hostTriages = _dbContext.TriageRecords
                .Include(t => t.Patient)
                .Where(t => campIds.Contains(t.CampId))
                .ToList();

            var patientsList = hostTriages
                .Select(t => t.Patient)
                .Where(p => p != null)
                .Distinct()
                .Cast<ApplicationUser>()
                .ToList();

            if (!patientsList.Any())
            {
                patientsList = _dbContext.Users.Where(u => u.Role == SystemRoles.Patient).Take(10).ToList();
            }

            int totalPatients = patientsList.Count;

            // District & Upazila distribution
            var districtDist = patientsList
                .GroupBy(p => p.District ?? "Unknown")
                .Select(g => new DemographicStatItem
                {
                    Label = g.Key,
                    Count = g.Count(),
                    Percentage = totalPatients > 0 ? Math.Round((double)g.Count() / totalPatients * 100, 1) : 0
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            var upazilaDist = patientsList
                .GroupBy(p => p.Upazila ?? "Unknown")
                .Select(g => new DemographicStatItem
                {
                    Label = g.Key,
                    Count = g.Count(),
                    Percentage = totalPatients > 0 ? Math.Round((double)g.Count() / totalPatients * 100, 1) : 0
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Age distribution
            int pediatric = 0, youth = 0, adult = 0, geriatric = 0;
            var now = DateTime.UtcNow;
            foreach (var p in patientsList)
            {
                if (p.DateOfBirth.HasValue)
                {
                    int age = now.Year - p.DateOfBirth.Value.Year;
                    if (now < p.DateOfBirth.Value.AddYears(age)) age--;

                    if (age < 15) pediatric++;
                    else if (age <= 24) youth++;
                    else if (age <= 59) adult++;
                    else geriatric++;
                }
                else
                {
                    adult++; // default fallback
                }
            }

            var ageDist = new List<DemographicStatItem>
            {
                new DemographicStatItem { Label = "Pediatric (<15 yrs)", Count = pediatric, Percentage = totalPatients > 0 ? Math.Round((double)pediatric / totalPatients * 100, 1) : 0 },
                new DemographicStatItem { Label = "Youth (15-24 yrs)", Count = youth, Percentage = totalPatients > 0 ? Math.Round((double)youth / totalPatients * 100, 1) : 0 },
                new DemographicStatItem { Label = "Adult (25-59 yrs)", Count = adult, Percentage = totalPatients > 0 ? Math.Round((double)adult / totalPatients * 100, 1) : 0 },
                new DemographicStatItem { Label = "Geriatric (60+ yrs)", Count = geriatric, Percentage = totalPatients > 0 ? Math.Round((double)geriatric / totalPatients * 100, 1) : 0 }
            };

            // Gender distribution
            var genderDist = patientsList
                .GroupBy(p => p.Gender ?? "Not Specified")
                .Select(g => new DemographicStatItem
                {
                    Label = g.Key,
                    Count = g.Count(),
                    Percentage = totalPatients > 0 ? Math.Round((double)g.Count() / totalPatients * 100, 1) : 0
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Blood Group distribution
            var bloodDist = patientsList
                .GroupBy(p => p.BloodGroup ?? "Unknown")
                .Select(g => new DemographicStatItem
                {
                    Label = g.Key,
                    Count = g.Count(),
                    Percentage = totalPatients > 0 ? Math.Round((double)g.Count() / totalPatients * 100, 1) : 0
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Urgency distribution
            var urgencyDist = hostTriages
                .GroupBy(t => t.UrgencyLevel)
                .Select(g => new DemographicStatItem
                {
                    Label = g.Key,
                    Count = g.Count(),
                    Percentage = hostTriages.Any() ? Math.Round((double)g.Count() / hostTriages.Count * 100, 1) : 0
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (!urgencyDist.Any())
            {
                urgencyDist.Add(new DemographicStatItem { Label = "Normal", Count = totalPatients, Percentage = 100 });
            }

            model.DemographicsReport = new HostDemographicsReportViewModel
            {
                TotalPatients = totalPatients,
                DistrictDistribution = districtDist,
                UpazilaDistribution = upazilaDist,
                AgeDistribution = ageDist,
                GenderDistribution = genderDist,
                BloodGroupDistribution = bloodDist,
                UrgencyDistribution = urgencyDist
            };

            return View(model);
        }

        // =========================================================================
        // 10. LOG CAMP EXPENSE BY CATEGORY
        // =========================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogExpense(CampExpenseInputModel model)
        {
            var currentHost = GetCurrentHostUser();
            if (currentHost == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == model.CampId && c.HostId == currentHost.Id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found or access denied.";
                return RedirectToAction(nameof(Reports), new { tab = "financial" });
            }

            if (!ModelState.IsValid || model.Amount <= 0)
            {
                TempData["ErrorMessage"] = "Please provide a valid category and positive expense amount.";
                return RedirectToAction(nameof(Reports), new { tab = "financial", campId = model.CampId });
            }

            var newExpense = new CampExpense
            {
                CampId = model.CampId,
                Category = model.Category,
                Amount = model.Amount,
                Description = model.Description?.Trim(),
                ExpenseDate = DateTime.SpecifyKind(model.ExpenseDate, DateTimeKind.Utc),
                LoggedByUserId = currentHost.Id
            };

            _dbContext.CampExpenses.Add(newExpense);
            camp.UtilizedBudget += model.Amount;
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"Expense of ৳{model.Amount:N2} under '{model.Category}' recorded successfully.";
            return RedirectToAction(nameof(Reports), new { tab = "financial", campId = model.CampId });
        }
    }
}

