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
                .Where(c => c.HostId == currentHost.Id || c.HostId == null || c.HostId == "usr-host-01")
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
                .Where(c => c.HostId == currentHost.Id || c.HostId == null || c.HostId == "usr-host-01")
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
        public IActionResult ManageStaff(int id, string? doctorSearch, string? volunteerSearch, int doctorPage = 1, int volunteerPage = 1)
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
            
            int pageSize = 6; // Changed to 6 so it displays well in a 2-column or 3-column grid

            var totalDoctors = doctorsQuery.Count();
            var totalVolunteers = volunteersQuery.Count();

            var model = new HostManageStaffViewModel
            {
                Camp = camp,
                CurrentRequests = _mockDataService.GetRequestsForCamp(id),
                AvailableDoctors = doctorsQuery.Skip((doctorPage - 1) * pageSize).Take(pageSize).ToList(),
                CurrentVolunteerRequests = _mockDataService.GetVolunteerRequestsForCamp(id),
                AvailableVolunteers = volunteersQuery.Skip((volunteerPage - 1) * pageSize).Take(pageSize).ToList(),
                
                CurrentDoctorPage = doctorPage,
                TotalDoctorPages = (int)Math.Ceiling(totalDoctors / (double)pageSize),
                DoctorPageSize = pageSize,

                CurrentVolunteerPage = volunteerPage,
                TotalVolunteerPages = (int)Math.Ceiling(totalVolunteers / (double)pageSize),
                VolunteerPageSize = pageSize
            };

            ViewBag.DoctorSearch = doctorSearch;
            ViewBag.VolunteerSearch = volunteerSearch;

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
    }
}
