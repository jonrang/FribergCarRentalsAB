using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FribergCarRentalsAB.Data;
using FribergCarRentalsAB.Models;

namespace FribergCarRentalsAB.Controllers
{
    public class CarController : Controller
    {
        private readonly ICarRepository carRepository;
        private readonly IWebHostEnvironment environment;

        public CarController(ICarRepository carRepository, IWebHostEnvironment environment)
        {
            this.carRepository = carRepository;
            this.environment = environment;
        }

        // GET: Car
        public async Task<IActionResult> Index()
        {
            var cars = await carRepository.GetAll();
            return View(cars);
        }

        // GET: Car/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await carRepository.GetById((int)id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // GET: Car/Create
        public IActionResult Create()
        {
            ViewBag.Images = GetImageSelectList();
            return View(new Car());
        }

        // POST: Car/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Make,Model,Year,LicensePlate,RatePerDay,Mileage,IsAvailable,ImageFileName")] Car car)
        {
            if (ModelState.IsValid)
            {
                await carRepository.Create(car);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Images = GetImageSelectList(car.ImageFileName);
            return View(car);
        }

        // GET: Car/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await carRepository.Find((int)id);
            if (car == null)
            {
                return NotFound();
            }
            ViewBag.Images = GetImageSelectList(car.ImageFileName);
            return View(car);
        }

        // POST: Car/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Make,Model,Year,LicensePlate,RatePerDay,Mileage,IsAvailable")] Car car)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await carRepository.Update(car);
                }
                catch (DbUpdateConcurrencyException)
                {

                    throw;

                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Images = GetImageSelectList(car.ImageFileName);
            return View(car);
        }

        // GET: Car/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await carRepository.Find((int)id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Car/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
           
            await carRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private SelectList GetImageSelectList(string? selected = null)
        {
            var imagesFolder = Path.Combine(environment.WebRootPath, "images", "cars");
            if (!Directory.Exists(imagesFolder))
                return new SelectList(Enumerable.Empty<string>());

            var files = Directory
              .GetFiles(imagesFolder)
              .Select(Path.GetFileName)   
              .OrderBy(n => n)
              .ToList();

            return new SelectList(files, selected);
        }

    }
}
