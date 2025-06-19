using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FribergCarRentalsAB.Data;


namespace FribergCarRentalsAB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RentalAdminController : Controller
    {
        private readonly IRentalService rentalService;
        private readonly IRentalRepository rentalRepository;

        public RentalAdminController(IRentalService rentalService, IRentalRepository rentalRepository)
        {
            this.rentalService = rentalService;
            this.rentalRepository = rentalRepository;
        }
        public async Task<IActionResult> Index()
        {
            var rentals = await rentalRepository.GetAllAsync();
            return View(rentals);
        }

        public async Task<IActionResult> Details(int id)
        {
            var rent = await rentalRepository.GetByIdAsync(id);
            if (rent == null) return NotFound();
            return View(rent);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            await rentalService.MarkReturnedAsync(id);
            TempData["Success"] = "Bokningen markerad som återlämnad.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            await rentalService.CancelRentalAsync(id);
            TempData["Success"] = "Bokningen avbokad.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await rentalRepository.Delete(id);
            TempData["Success"] = "Bokningen raderad.";
            return RedirectToAction(nameof(Index));
        }

    }
}
