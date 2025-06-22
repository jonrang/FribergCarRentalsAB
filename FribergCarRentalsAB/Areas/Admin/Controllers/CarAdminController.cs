using FribergCarRentalsAB.Data;
using FribergCarRentalsAB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace FribergCarRentalsAB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CarAdminController : Controller
    {
        private readonly ICarRepository carRepository;
        private readonly IWebHostEnvironment environment;
        private readonly string imageFolder;

        public CarAdminController(ICarRepository carRepository, IWebHostEnvironment environment)
        {
            this.carRepository = carRepository;
            this.environment = environment;
            imageFolder = Path.Combine(environment.WebRootPath, "images", "cars");
        }
        // GET: Admin/Car
        public async Task<IActionResult> Index()
        {
            var cars = await carRepository.GetAll();
            return View(cars);
        }

        // GET: Admin/Car/Create
        public IActionResult Create()
        {
            var vm = new CarCreateViewModel
            {
                ExistingImages = GetExistingImagesSelectList()
            };
            return View(vm);
        }


        // POST: Admin/Car/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.ExistingImages = GetExistingImagesSelectList(vm.SelectedImageFileName);
                return View(vm);
            }

            // Determine filename: uploaded or existing
            string fileName = vm.ImageUpload != null
                ? await SaveUploadedFile(vm.ImageUpload)
                : vm.SelectedImageFileName ?? string.Empty;

            var car = new Car
            {
                Make = vm.Make,
                Model = vm.Model,
                Year = vm.Year,
                LicensePlate = vm.LicensePlate,
                Mileage = vm.Mileage,
                RatePerDay = vm.RatePerDay,
                ImageFileName = fileName,
                IsAvailable = true     // default new cars available
            };

            await carRepository.Create(car);
            TempData["Success"] = "New car added successfully.";
            return RedirectToAction(nameof(Index));
        }


        // GET: Admin/Car/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var car = await carRepository.GetById(id);
            if (car == null) return NotFound();

            var vm = new CarEditViewModel
            {
                Id = car.Id,
                Make = car.Make,
                Model = car.Model,
                Year = car.Year,
                LicensePlate = car.LicensePlate,
                Mileage = car.Mileage,
                RatePerDay = car.RatePerDay,
                CurrentImageFileName = car.ImageFileName,
                IsAvailable = car.IsAvailable,
                ExistingImages = GetExistingImagesSelectList(car.ImageFileName),
                SelectedImageFileName = car.ImageFileName
            };
            return View(vm);
        }

        // POST: Admin/Car/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarEditViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                vm.ExistingImages = GetExistingImagesSelectList(vm.SelectedImageFileName);
                return View(vm);
            }

            var car = await carRepository.GetById(id);
            if (car == null) return NotFound();

            // Decide which filename to use
            car.ImageFileName = vm.ImageUpload != null
                ? await SaveUploadedFile(vm.ImageUpload)
                : vm.SelectedImageFileName ?? car.ImageFileName;

            car.Make = vm.Make;
            car.Model = vm.Model;
            car.Year = vm.Year;
            car.LicensePlate = vm.LicensePlate;
            car.Mileage = vm.Mileage;
            car.RatePerDay = vm.RatePerDay;
            car.IsAvailable = vm.IsAvailable;

            await carRepository.Update(car);
            TempData["Success"] = "Car updated successfully.";
            return RedirectToAction(nameof(Index));
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await carRepository.Delete(id);
            TempData["Success"] = "Car deleted.";
            return RedirectToAction(nameof(Index));

        }

        private SelectList GetExistingImagesSelectList(string? selected = null)
        {
            if (!Directory.Exists(imageFolder))
                return new SelectList(Enumerable.Empty<string>());

            var files = Directory
              .EnumerateFiles(imageFolder, "*.jpg")
              .Select(Path.GetFileName)
              .OrderBy(n => n);
            return new SelectList(files, selected);
        }

        private async Task<string> SaveUploadedFile(IFormFile file)
        {
            // ensure folder exists
            Directory.CreateDirectory(imageFolder);

            // unique filename
            var fileName = Guid.NewGuid()
                         + Path.GetExtension(file.FileName);

            var dest = Path.Combine(imageFolder, fileName);
            using var stream = new FileStream(dest, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }



    }
}
