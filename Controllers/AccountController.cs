using System.Security.Claims;
using MediCamp.Models;
using MediCamp.Models.ViewModels;
using MediCamp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCamp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMockDataService _dataService;
        private readonly MediCamp.Data.ApplicationDbContext _context;

        public AccountController(IMockDataService dataService, MediCamp.Data.ApplicationDbContext context)
        {
            _dataService = dataService;
            _context = context;
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

        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            var user = _dataService.GetUserById(userId);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            var user = _dataService.GetUserById(userId);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                NID = user.NID,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                BloodGroup = user.BloodGroup,
                District = user.District,
                Upazila = user.Upazila,
                Address = user.Address,
                MedicalSpecialization = user.MedicalSpecialization,
                BMDCRegNo = user.BMDCRegNo,
                OrganizationName = user.OrganizationName,
                OrganizationType = user.OrganizationType,
                OrganizationRegNo = user.OrganizationRegNo,
                FocalPersonContact = user.FocalPersonContact
            };

            ViewBag.Role = user.Role;
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(EditProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");
            
            var user = _dataService.GetUserById(userId);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Role = user.Role;
                return View(model);
            }

            var (success, message) = _dataService.UpdateUserProfile(userId, model);
            
            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Profile));
            }
            else
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.Role = user.Role;
                return View(model);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PatientHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

            // Fetch triage records along with nested consultations and prescriptions
            var history = await _context.TriageRecords
                .Include(t => t.Camp)
                .Include(t => t.Volunteer)
                .Where(t => t.PatientId == userId)
                .OrderByDescending(t => t.RecordedAt)
                .ToListAsync();

            // Load consultations and prescriptions manually since EF Core Include chain can sometimes be tricky or heavy
            foreach (var triage in history)
            {
                var consultation = await _context.Consultations
                    .Include(c => c.Doctor)
                    .FirstOrDefaultAsync(c => c.TriageRecordId == triage.Id);
                    
                if (consultation != null)
                {
                    // Manually assign it to avoid relying on lazy loading if it's disabled
                    // But we can just query it and pass it to a ViewModel.
                    // Wait, TriageRecord doesn't have a Consultation navigation property, Consultation has TriageRecordId!
                }
            }

            // A better way: Use a ViewModel to map everything.
            var viewModels = new List<PatientHistoryViewModel>();

            foreach (var t in history)
            {
                var consultation = await _context.Consultations
                    .Include(c => c.Doctor)
                    .FirstOrDefaultAsync(c => c.TriageRecordId == t.Id);
                    
                var prescriptions = new List<MediCamp.Models.Domain.PrescriptionItem>();
                if (consultation != null)
                {
                    var rx = await _context.Prescriptions
                        .FirstOrDefaultAsync(p => p.ConsultationId == consultation.Id);
                        
                    if (rx != null)
                    {
                        prescriptions = await _context.PrescriptionItems
                            .Include(pi => pi.MasterMedicine)
                            .Where(pi => pi.PrescriptionId == rx.Id)
                            .ToListAsync();
                    }
                }

                viewModels.Add(new PatientHistoryViewModel
                {
                    TriageRecord = t,
                    Consultation = consultation,
                    PrescriptionItems = prescriptions
                });
            }

            return View(viewModels);
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
