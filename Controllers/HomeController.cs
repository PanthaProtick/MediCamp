using System.Diagnostics;
using System.Security.Claims;
using MediCamp.Data;
using MediCamp.Models;
using MediCamp.Models.ViewModels;
using MediCamp.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediCamp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMockDataService _dataService;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, IMockDataService dataService, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dataService = dataService;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var model = _dataService.GetHomeLandingData();
            return View(model);
        }

        public IActionResult Camps(string? district, string? upazila, DateTime? startDate, string? search)
        {
            var camps = _dataService.GetAllCamps()
                .Where(c => c.Status == "Scheduled" || c.Status == "Ongoing" || c.Status == "Completed")
                .ToList();

            if (!string.IsNullOrWhiteSpace(district) && district != "All")
            {
                camps = camps.Where(c => c.District.Equals(district, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(upazila) && upazila != "All")
            {
                camps = camps.Where(c => c.Upazila.Equals(upazila, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (startDate.HasValue)
            {
                camps = camps.Where(c => c.StartDate.Date >= startDate.Value.Date).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLowerInvariant();
                camps = camps.Where(c => c.Title.ToLowerInvariant().Contains(term) ||
                                         c.Venue.ToLowerInvariant().Contains(term) ||
                                         c.HostOrganization.ToLowerInvariant().Contains(term)).ToList();
            }

            ViewBag.SelectedDistrict = district;
            ViewBag.SelectedUpazila = upazila;
            ViewBag.SelectedDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.SearchTerm = search;

            // Extract distinct values for dropdowns
            ViewBag.Districts = _dataService.GetAllCamps().Select(c => c.District).Distinct().OrderBy(d => d).ToList();
            
            var upazilasQuery = _dataService.GetAllCamps().AsEnumerable();
            if (!string.IsNullOrWhiteSpace(district) && district != "All")
            {
                upazilasQuery = upazilasQuery.Where(c => c.District.Equals(district, StringComparison.OrdinalIgnoreCase));
            }
            ViewBag.Upazilas = upazilasQuery.Select(c => c.Upazila).Distinct().OrderBy(u => u).ToList();

            // Populate user's status for camps if logged in
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                if (User.IsInRole(SystemRoles.Pharmacist))
                {
                    ViewBag.UserCampStatus = _dbContext.CampPharmacistRequests
                        .Where(r => r.PharmacistId == userId)
                        .AsEnumerable()
                        .GroupBy(r => r.CampId)
                        .ToDictionary(g => g.Key, g => g.First().Status);
                }
                else if (User.IsInRole(SystemRoles.Doctor))
                {
                    ViewBag.UserCampStatus = _dbContext.CampStaffRequests
                        .Where(r => r.DoctorId == userId)
                        .AsEnumerable()
                        .GroupBy(r => r.CampId)
                        .ToDictionary(g => g.Key, g => g.First().Status);
                }
                else if (User.IsInRole(SystemRoles.Volunteer))
                {
                    ViewBag.UserCampStatus = _dbContext.CampVolunteerRequests
                        .Where(r => r.VolunteerId == userId)
                        .AsEnumerable()
                        .GroupBy(r => r.CampId)
                        .ToDictionary(g => g.Key, g => g.First().Status);
                }
                else if (User.IsInRole(SystemRoles.Patient))
                {
                    ViewBag.UserCampStatus = _dbContext.CampPatientRegistrations
                        .Where(r => r.PatientId == userId && r.Status == "Registered")
                        .AsEnumerable()
                        .GroupBy(r => r.CampId)
                        .ToDictionary(g => g.Key, g => g.First().Status);
                }
            }

            return View(camps);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
