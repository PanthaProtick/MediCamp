using MediCamp.Data;
using MediCamp.Models;
using MediCamp.Models.Domain;
using MediCamp.Models.ViewModels;
using MediCamp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId)) return userId;

            if (string.IsNullOrEmpty(userEmail)) return null;
            var user = _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == userEmail.ToLower());
            if (user != null) return user.Id;
            
            // Fallback to mock service
            var mockUser = _mockDataService.GetAllUsers().FirstOrDefault(u => u.Role == SystemRoles.Doctor);
            return mockUser?.Id;
        }

        [HttpGet]
        public IActionResult Requests()
        {
            var doctorId = GetCurrentDoctorId();
            if (string.IsNullOrEmpty(doctorId))
            {
                return RedirectToAction("Login", "Account");
            }

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
            {
                return RedirectToAction("Login", "Account");
            }

            var result = _mockDataService.RespondToCampStaffRequest(requestId, doctorId, status);

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
