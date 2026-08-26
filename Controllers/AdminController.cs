using MediCamp.Models;
using MediCamp.Models.ViewModels;
using MediCamp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCamp.Controllers
{
    [Authorize(Roles = SystemRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly IMockDataService _dataService;
        private readonly MediCamp.Data.ApplicationDbContext _dbContext;

        public AdminController(IMockDataService dataService, MediCamp.Data.ApplicationDbContext dbContext)
        {
            _dataService = dataService;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult HostApprovals(string tab = "Pending", string? search = null)
        {
            var allHosts = _dataService.GetHostsByStatus("All");
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                string query = search.Trim().ToLower();
                allHosts = allHosts.Where(h => 
                    (h.OrganizationName != null && h.OrganizationName.ToLower().Contains(query)) ||
                    h.FullName.ToLower().Contains(query) ||
                    h.Email.ToLower().Contains(query) ||
                    (h.OrganizationRegNo != null && h.OrganizationRegNo.ToLower().Contains(query)) ||
                    (h.OrganizationType != null && h.OrganizationType.ToLower().Contains(query))
                ).ToList();
            }

            var pending = allHosts.Where(h => h.HostApprovalStatus == "Pending").ToList();
            var approved = allHosts.Where(h => h.HostApprovalStatus == "Approved").ToList();
            var rejected = allHosts.Where(h => h.HostApprovalStatus == "Rejected").ToList();

            var model = new HostApprovalsViewModel
            {
                PendingHosts = pending,
                ApprovedHosts = approved,
                RejectedHosts = rejected,
                AllHosts = allHosts,
                ActiveTab = string.IsNullOrWhiteSpace(tab) ? "Pending" : tab,
                SearchTerm = search
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveHost(string userId)
        {
            var (success, message) = _dataService.ApproveHost(userId);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(HostApprovals), new { tab = "Pending" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectHost(string userId, string rejectionReason)
        {
            var (success, message) = _dataService.RejectHost(userId, rejectionReason);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(HostApprovals), new { tab = "Pending" });
        }

        [HttpGet]
        public IActionResult UserManagement(string? search, string? role, string? status)
        {
            var filteredUsers = _dataService.GetFilteredUsers(search, role, status);
            var allUsers = _dataService.GetAllUsers();

            var model = new UserManagementViewModel
            {
                Users = filteredUsers,
                SearchTerm = search,
                SelectedRole = role,
                SelectedStatus = status,
                TotalUsersCount = allUsers.Count,
                ActiveAdminsCount = allUsers.Count(u => u.Role == SystemRoles.Admin && u.IsActive),
                ActiveHostsCount = allUsers.Count(u => u.Role == SystemRoles.Host && u.IsActive),
                ActiveDoctorsCount = allUsers.Count(u => u.Role == SystemRoles.Doctor && u.IsActive),
                ActiveVolunteersCount = allUsers.Count(u => u.Role == SystemRoles.Volunteer && u.IsActive),
                ActivePharmacistsCount = allUsers.Count(u => u.Role == SystemRoles.Pharmacist && u.IsActive),
                ActivePatientsCount = allUsers.Count(u => u.Role == SystemRoles.Patient && u.IsActive),
                PendingApprovalsCount = allUsers.Count(u => u.HostApprovalStatus == "Pending")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide all required fields correctly.";
                return RedirectToAction(nameof(UserManagement));
            }

            var (success, message) = _dataService.CreateUser(model);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeRole(string userId, string newRole)
        {
            var (success, message) = _dataService.UpdateUserRole(userId, newRole);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(string userId)
        {
            var (success, message) = _dataService.ToggleUserStatus(userId);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "New password must be at least 6 characters long.";
                return RedirectToAction(nameof(UserManagement));
            }

            var (success, message) = _dataService.ResetUserPassword(userId, newPassword);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(string userId)
        {
            var (success, message) = _dataService.DeleteUser(userId);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(UserManagement));
        }

        [HttpGet]
        public IActionResult GetUserDetails(string id)
        {
            var user = _dataService.GetUserById(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Json(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                nid = user.NID ?? "N/A",
                role = user.Role,
                district = user.District ?? "N/A",
                upazila = user.Upazila ?? "N/A",
                address = user.Address ?? "N/A",
                bloodGroup = user.BloodGroup ?? "N/A",
                gender = user.Gender ?? "N/A",
                organizationName = user.OrganizationName ?? "N/A",
                organizationRegNo = user.OrganizationRegNo ?? "N/A",
                medicalSpecialization = user.MedicalSpecialization ?? "N/A",
                bmdcRegNo = user.BMDCRegNo ?? "N/A",
                isActive = user.IsActive,
                hostApprovalStatus = user.HostApprovalStatus,
                createdAt = user.CreatedAt.ToString("dd MMM yyyy, hh:mm tt"),
                lastLoginAt = user.LastLoginAt.HasValue ? user.LastLoginAt.Value.ToString("dd MMM yyyy, hh:mm tt") : "Never logged in"
            });
        }
        [HttpGet]
        public IActionResult CampApprovals(string tab = "Pending")
        {
            var campsQuery = _dbContext.Camps.Include(c => c.Host).ToList();

            var viewModels = campsQuery.Select(c => {
                var vm = new AdminCampApprovalItemViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    CampType = c.CampType,
                    HostOrganizationName = c.Host?.OrganizationName ?? c.Host?.FullName ?? "Unknown",
                    District = c.District,
                    Upazila = c.Upazila,
                    Venue = c.Venue,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    ExpectedPatients = c.ExpectedPatients,
                    TotalBudget = c.TotalBudget,
                    SubmittedDate = c.CreatedAt,
                    CampRejectionReason = c.CampRejectionReason
                };

                if (c.Status == "Pending Admin Approval")
                {
                    // Conflict Check
                    var overlapping = _dbContext.Camps.Any(other => 
                        other.Id != c.Id && 
                        other.District == c.District && 
                        other.Upazila == c.Upazila &&
                        (other.Status == "Scheduled" || other.Status == "Ongoing") &&
                        other.StartDate <= c.EndDate && other.EndDate >= c.StartDate);
                    
                    if (overlapping)
                    {
                        vm.HasDateConflict = true;
                        vm.ConflictWarningMessage = $"Date conflict detected with another Scheduled/Ongoing camp in {c.Upazila}, {c.District}.";
                    }

                    // Capacity Check
                    if (c.ExpectedPatients > 0)
                    {
                        decimal costPerPatient = c.TotalBudget / c.ExpectedPatients;
                        if (costPerPatient < 50 || costPerPatient > 2000)
                        {
                            vm.HasCapacitySanityFlag = true;
                            vm.CapacitySanityMessage = $"Budget per patient is unusually {(costPerPatient < 50 ? "low" : "high")} (৳{costPerPatient:N2}) for {c.CampType}.";
                        }
                    }
                }

                return vm;
            }).ToList();

            var model = new AdminCampApprovalsViewModel
            {
                ActiveTab = string.IsNullOrWhiteSpace(tab) ? "Pending" : tab,
                PendingCamps = viewModels.Where(c => campsQuery.First(q => q.Id == c.Id).Status == "Pending Admin Approval").ToList(),
                ScheduledCamps = viewModels.Where(c => campsQuery.First(q => q.Id == c.Id).Status == "Scheduled").ToList(),
                RejectedCamps = viewModels.Where(c => campsQuery.First(q => q.Id == c.Id).Status == "Rejected").ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveCamp(int id)
        {
            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found.";
                return RedirectToAction(nameof(CampApprovals), new { tab = "Pending" });
            }

            if (camp.Status != "Pending Admin Approval")
            {
                TempData["ErrorMessage"] = "Camp is no longer pending approval.";
                return RedirectToAction(nameof(CampApprovals), new { tab = "Pending" });
            }

            camp.Status = "Scheduled";
            camp.CampRejectionReason = null;
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"Camp '{camp.Title}' has been approved and published to the directory.";
            return RedirectToAction(nameof(CampApprovals), new { tab = "Pending" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectCamp(int id, string rejectionReason)
        {
            var camp = _dbContext.Camps.FirstOrDefault(c => c.Id == id);
            if (camp == null)
            {
                TempData["ErrorMessage"] = "Camp not found.";
                return RedirectToAction(nameof(CampApprovals), new { tab = "Pending" });
            }

            if (camp.Status != "Pending Admin Approval")
            {
                TempData["ErrorMessage"] = "Camp is no longer pending approval.";
                return RedirectToAction(nameof(CampApprovals), new { tab = "Pending" });
            }

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["ErrorMessage"] = "A rejection reason is required.";
                return RedirectToAction(nameof(CampApprovals), new { tab = "Pending" });
            }

            camp.Status = "Rejected";
            camp.CampRejectionReason = rejectionReason;
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = $"Camp '{camp.Title}' has been rejected. The host will be notified.";
            return RedirectToAction(nameof(CampApprovals), new { tab = "Pending" });
        }
    }
}
