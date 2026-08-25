using System.Diagnostics;
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

        public HomeController(ILogger<HomeController> logger, IMockDataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var model = _dataService.GetHomeLandingData();
            return View(model);
        }

        public IActionResult Camps(string? district, string? campType, string? search)
        {
            var camps = _dataService.GetAllCamps();

            if (!string.IsNullOrWhiteSpace(district) && district != "All")
            {
                camps = camps.Where(c => c.District.Equals(district, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(campType) && campType != "All")
            {
                camps = camps.Where(c => c.CampType.Contains(campType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLowerInvariant();
                camps = camps.Where(c => c.Title.ToLowerInvariant().Contains(term) ||
                                         c.Venue.ToLowerInvariant().Contains(term) ||
                                         c.HostOrganization.ToLowerInvariant().Contains(term)).ToList();
            }

            ViewBag.SelectedDistrict = district;
            ViewBag.SelectedType = campType;
            ViewBag.SearchTerm = search;

            return View(camps);
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
