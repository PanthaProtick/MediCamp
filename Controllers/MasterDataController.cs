using MediCamp.Data;
using MediCamp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCamp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MasterDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MasterDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =======================
        // LOCATIONS
        // =======================
        public async Task<IActionResult> Locations()
        {
            var locations = await _context.Locations.ToListAsync();
            return View(locations);
        }

        [HttpGet]
        public IActionResult CreateLocation() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(Location location)
        {
            if (ModelState.IsValid)
            {
                _context.Add(location);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Location created successfully.";
                return RedirectToAction(nameof(Locations));
            }
            return View(location);
        }

        [HttpGet]
        public async Task<IActionResult> EditLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLocation(int id, Location location)
        {
            if (id != location.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(location);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Location updated successfully.";
                return RedirectToAction(nameof(Locations));
            }
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location != null)
            {
                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Location deleted.";
            }
            return RedirectToAction(nameof(Locations));
        }

        // =======================
        // MEDICINES
        // =======================
        public async Task<IActionResult> Medicines()
        {
            var medicines = await _context.MasterMedicines.ToListAsync();
            return View(medicines);
        }

        [HttpGet]
        public IActionResult CreateMedicine() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedicine(MasterMedicine medicine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(medicine);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Medicine created successfully.";
                return RedirectToAction(nameof(Medicines));
            }
            return View(medicine);
        }

        [HttpGet]
        public async Task<IActionResult> EditMedicine(int id)
        {
            var medicine = await _context.MasterMedicines.FindAsync(id);
            if (medicine == null) return NotFound();
            return View(medicine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicine(int id, MasterMedicine medicine)
        {
            if (id != medicine.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(medicine);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Medicine updated successfully.";
                return RedirectToAction(nameof(Medicines));
            }
            return View(medicine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicine(int id)
        {
            var medicine = await _context.MasterMedicines.FindAsync(id);
            if (medicine != null)
            {
                _context.MasterMedicines.Remove(medicine);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Medicine deleted.";
            }
            return RedirectToAction(nameof(Medicines));
        }

        // =======================
        // HOSPITALS
        // =======================
        public async Task<IActionResult> Hospitals()
        {
            var hospitals = await _context.Hospitals.ToListAsync();
            return View(hospitals);
        }

        [HttpGet]
        public IActionResult CreateHospital() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHospital(Hospital hospital)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hospital);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hospital created successfully.";
                return RedirectToAction(nameof(Hospitals));
            }
            return View(hospital);
        }

        [HttpGet]
        public async Task<IActionResult> EditHospital(int id)
        {
            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital == null) return NotFound();
            return View(hospital);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHospital(int id, Hospital hospital)
        {
            if (id != hospital.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(hospital);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hospital updated successfully.";
                return RedirectToAction(nameof(Hospitals));
            }
            return View(hospital);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHospital(int id)
        {
            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital != null)
            {
                _context.Hospitals.Remove(hospital);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hospital deleted.";
            }
            return RedirectToAction(nameof(Hospitals));
        }

        // =======================
        // BLOOD GROUPS
        // =======================
        public async Task<IActionResult> BloodGroups()
        {
            var groups = await _context.BloodGroupMasters.ToListAsync();
            return View(groups);
        }

        [HttpGet]
        public IActionResult CreateBloodGroup() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBloodGroup(BloodGroupMaster bg)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bg);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Blood Group created successfully.";
                return RedirectToAction(nameof(BloodGroups));
            }
            return View(bg);
        }

        [HttpGet]
        public async Task<IActionResult> EditBloodGroup(int id)
        {
            var bg = await _context.BloodGroupMasters.FindAsync(id);
            if (bg == null) return NotFound();
            return View(bg);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBloodGroup(int id, BloodGroupMaster bg)
        {
            if (id != bg.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(bg);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Blood Group updated successfully.";
                return RedirectToAction(nameof(BloodGroups));
            }
            return View(bg);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBloodGroup(int id)
        {
            var bg = await _context.BloodGroupMasters.FindAsync(id);
            if (bg != null)
            {
                _context.BloodGroupMasters.Remove(bg);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Blood Group deleted.";
            }
            return RedirectToAction(nameof(BloodGroups));
        }
    }
}
