using System.Security.Claims;
using MediCamp.Models;
using MediCamp.Models.ViewModels;
using MediCamp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace MediCamp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMockDataService _dataService;

        public AccountController(IMockDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectBasedOnRole(User.FindFirstValue(ClaimTypes.Role));
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message, user) = _dataService.Authenticate(model.Identifier, model.Password);

            if (!success || user == null)
            {
                ModelState.AddModelError(string.Empty, message);
                model.ErrorMessage = message;
                return View(model);
            }

            await SignInUserAsync(user, model.RememberMe);
            TempData["SuccessMessage"] = $"Welcome back, {user.FullName}! You are signed in as {user.Role}.";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectBasedOnRole(user.Role);
        }

        // =========================================================================
        // DEDICATED ROLE-SPECIFIC REGISTRATION ROUTES
        // =========================================================================

        [HttpGet]
        public IActionResult Register(string? role = null)
        {
            return role switch
            {
                SystemRoles.Doctor => RedirectToAction(nameof(RegisterDoctor)),
                SystemRoles.Host => RedirectToAction(nameof(RegisterHost)),
                SystemRoles.Volunteer => RedirectToAction(nameof(RegisterVolunteer)),
                SystemRoles.Pharmacist => RedirectToAction(nameof(RegisterPharmacist)),
                _ => RedirectToAction(nameof(RegisterPatient))
            };
        }

        // 1. Patient Registration
        [HttpGet]
        public IActionResult RegisterPatient(bool donor = false)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            var model = new RegisterPatientViewModel { IsBloodDonor = donor };
            return View("RegisterPatient", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPatient(RegisterPatientViewModel model)
        {
            model.Role = SystemRoles.Patient;
            if (!ModelState.IsValid) return View("RegisterPatient", model);

            var (success, message, user) = _dataService.RegisterUser(model);
            if (!success || user == null)
            {
                ModelState.AddModelError(string.Empty, message);
                return View("RegisterPatient", model);
            }

            await SignInUserAsync(user, false);
            TempData["SuccessMessage"] = "Patient registration completed successfully! Welcome to MediCamp.";
            return RedirectToAction("Index", "Home");
        }

        // 2. Doctor Registration
        [HttpGet]
        public IActionResult RegisterDoctor()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            var model = new RegisterViewModel { Role = SystemRoles.Doctor };
            return View("RegisterDoctor", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterDoctor(RegisterViewModel model)
        {
            model.Role = SystemRoles.Doctor;
            if (string.IsNullOrWhiteSpace(model.BMDCRegNo))
            {
                ModelState.AddModelError("BMDCRegNo", "BMDC Registration Number is required for doctor accounts.");
                return View("RegisterDoctor", model);
            }

            var (success, message, user) = _dataService.RegisterUser(model);
            if (!success || user == null)
            {
                ModelState.AddModelError(string.Empty, message);
                return View("RegisterDoctor", model);
            }

            await SignInUserAsync(user, false);
            TempData["SuccessMessage"] = "Doctor registration completed! Welcome to the MediCamp Clinical Portal.";
            return RedirectToAction("Index", "Home");
        }

        // 3. Host NGO Registration
        [HttpGet]
        public IActionResult RegisterHost()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            var model = new RegisterHostViewModel();
            return View("RegisterHost", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterHost(RegisterHostViewModel model)
        {
            model.Role = SystemRoles.Host;
            if (string.IsNullOrWhiteSpace(model.OrganizationName))
            {
                ModelState.AddModelError("OrganizationName", "Organization Name is required.");
                return View("RegisterHost", model);
            }

            var (success, message, user) = _dataService.RegisterHost(model);
            if (!success || user == null)
            {
                ModelState.AddModelError(string.Empty, message);
                return View("RegisterHost", model);
            }

            TempData["SuccessMessage"] = "Host Organization registration submitted successfully! Pending System Admin approval.";
            return View("HostRegistrationPending", user);
        }

        [HttpGet]
        public IActionResult HostRegistrationPending()
        {
            return View();
        }

        // 4. Volunteer Registration
        [HttpGet]
        public IActionResult RegisterVolunteer()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            var model = new RegisterViewModel { Role = SystemRoles.Volunteer };
            return View("RegisterVolunteer", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterVolunteer(RegisterViewModel model)
        {
            model.Role = SystemRoles.Volunteer;
            var (success, message, user) = _dataService.RegisterUser(model);
            if (!success || user == null)
            {
                ModelState.AddModelError(string.Empty, message);
                return View("RegisterVolunteer", model);
            }

            await SignInUserAsync(user, false);
            TempData["SuccessMessage"] = "Field Volunteer registration completed! Welcome to MediCamp Triage.";
            return RedirectToAction("Index", "Home");
        }

        // 5. Pharmacist Registration
        [HttpGet]
        public IActionResult RegisterPharmacist()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            var model = new RegisterViewModel { Role = SystemRoles.Pharmacist };
            return View("RegisterPharmacist", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPharmacist(RegisterViewModel model)
        {
            model.Role = SystemRoles.Pharmacist;
            var (success, message, user) = _dataService.RegisterUser(model);
            if (!success || user == null)
            {
                ModelState.AddModelError(string.Empty, message);
                return View("RegisterPharmacist", model);
            }

            await SignInUserAsync(user, false);
            TempData["SuccessMessage"] = "Pharmacist registration completed! Welcome to MediCamp Dispensary.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "You have been securely signed out.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUserAsync(ApplicationUser user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new("NID", user.NID ?? string.Empty),
                new("BloodGroup", user.BloodGroup ?? string.Empty),
                new("District", user.District ?? string.Empty),
                new("OrganizationName", user.OrganizationName ?? string.Empty)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private IActionResult RedirectBasedOnRole(string? role)
        {
            return role switch
            {
                SystemRoles.Admin => RedirectToAction("UserManagement", "Admin"),
                SystemRoles.Host => RedirectToAction("Dashboard", "Host"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}
