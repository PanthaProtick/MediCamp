using MediCamp.Models;
using MediCamp.Models.ViewModels;
using MediCamp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCamp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IMockDataService _dataService;

        public AdminController(IMockDataService dataService)
        {
            _dataService = dataService;
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
    }
}
