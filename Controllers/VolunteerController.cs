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
        public IActionResult Dashboard(int? campId, string? searchQuery)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var model = new VolunteerDashboardViewModel
            {
                SearchQuery = searchQuery,
                SelectedCampId = campId
            };

            // Get all approved camps for this volunteer
            var volunteerRequests = _mockDataService.GetRequestsForVolunteer(userId);
            model.ApprovedCamps = volunteerRequests
                .Where(r => r.Status == "Approved" && r.Camp != null)
                .Select(r => r.Camp!)
                .ToList();

            if (campId.HasValue)
            {
                model.SelectedCamp = model.ApprovedCamps.FirstOrDefault(c => c.Id == campId.Value);
                if (model.SelectedCamp == null)
                {
                    // Volunteer is not approved for this camp or it doesn't exist
                    return RedirectToAction("Dashboard");
                }

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    model.PatientSearchResults = _mockDataService.GetFilteredUsers(searchQuery, SystemRoles.Patient, "All");
                }
            }

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
    }
}
