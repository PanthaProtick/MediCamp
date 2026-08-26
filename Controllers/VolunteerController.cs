using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public VolunteerController(IMockDataService mockDataService)
        {
            _mockDataService = mockDataService;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet]
        public IActionResult Dashboard(string searchQuery)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var patients = new List<ApplicationUser>();
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                patients = _mockDataService.GetFilteredUsers(searchQuery, SystemRoles.Patient, "All");
            }

            ViewData["SearchQuery"] = searchQuery;
            return View(patients);
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
    }
}
