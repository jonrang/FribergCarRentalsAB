using System.Diagnostics;
using FribergCarRentalsAB.Data;
using FribergCarRentalsAB.Models;
using Microsoft.AspNetCore.Mvc;


namespace FribergCarRentalsAB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment hostEnvironment;
        private readonly IDbSeeder seeder;

        public HomeController(ILogger<HomeController> logger,
            IWebHostEnvironment hostEnvironment,
            IDbSeeder seeder)
        {
            _logger = logger;
            this.hostEnvironment = hostEnvironment;
            this.seeder = seeder;
        }

        public IActionResult Index()
        {
            var folder = Path.Combine(hostEnvironment.WebRootPath, "images", "cars");
            var files = Directory.Exists(folder) ? Directory
                           .EnumerateFiles(folder)
                           .Select(Path.GetFileName)
                           .ToList()
                       : new List<string>();
            var randomSix = files.OrderBy(_ => System.Guid
                .NewGuid())
                .Take(6)
                .ToList();

            var vm = new HomeIndexViewModel
            {
                CarImages = randomSix
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedCars()
        {
            var count = await seeder.SeedCarsFromJsonAsync();
            TempData["Success"] = count > 0
              ? $"{count} bilar tillagda från seed-filen."
              : "Inga nya bilar hittades att lägga till.";
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Privacy()
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
