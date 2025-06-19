using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
using System.Threading.Tasks;
using FribergCarRentalsAB.Data;
using FribergCarRentalsAB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FribergCarRentalsAB.Controllers
{
    [Authorize]
    public class RentalController : Controller
    {
        private readonly IRentalRepository rentalRepository;
        private readonly IRentalService rentalService;
        private readonly UserManager<ApplicationUser> userManager;

        public RentalController(IRentalRepository rentalRepository,
            IRentalService rentalService,
            UserManager<ApplicationUser> userManager)
        {
            this.rentalRepository = rentalRepository;
            this.rentalService = rentalService;
            this.userManager = userManager;
        }

        // GET: /Rental
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var rentals = await rentalService.GetActiveRentalsAsync(userId);
            var oldrentals = await rentalService.GetOldRentalsAsync(userId);

            var vm = new RentalIndexViewModel
            {
                Current = rentals,
                History = oldrentals
            };


            return View(vm);
        }


        // GET: /Rental/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var rental = await rentalRepository.GetByIdAsync(id);
            if (rental == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (rental.UserId != userId) return Forbid();

            return View(rental);
        }


        // POST: /Rental/Return/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var rental = await rentalRepository.GetByIdAsync(id);
            if (rental.UserId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
                return Forbid();

            await rentalService.MarkReturnedAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Rental/Cancel/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var rental = await rentalRepository.GetByIdAsync(id);
            if (rental.UserId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
                return Forbid();
            await rentalService.CancelRentalAsync(id);
            return RedirectToAction(nameof(Index));
        }



        // GET: /Rental/Create
        public async Task<IActionResult> Create(int? carId)
        {
            
            ViewBag.Cars = await rentalService.GetAvailableCarsAsync(carId);

            var vm = new RentalInputModel
            {
                CarId = carId ?? 0,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };

            return View(vm);
        }



        // POST: /Rental/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalInputModel m)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cars = await rentalService.GetAvailableCarsAsync(m.CarId, m.StartDate, m.EndDate);
                return View(m);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var rental = await rentalService.CreateRentalAsync(
                userId, m.CarId, m.StartDate, m.EndDate);

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }
    }
}
