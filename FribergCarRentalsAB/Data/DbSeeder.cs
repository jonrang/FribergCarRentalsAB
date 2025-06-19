using System.Text.Json;
using FribergCarRentalsAB.Data;
using FribergCarRentalsAB.Models;

public class DbSeeder : IDbSeeder
{
    private readonly IWebHostEnvironment _env;
    private readonly ApplicationDbContext _ctx;

    public DbSeeder(IWebHostEnvironment env, ApplicationDbContext ctx)
    {
        _env = env;
        _ctx = ctx;
    }

    public async Task<int> SeedCarsFromJsonAsync()
    {
        var path = Path.Combine(_env.ContentRootPath, "Data", "seed-cars.json");
        if (!File.Exists(path)) return 0;

        var json = await File.ReadAllTextAsync(path);
        var cars = JsonSerializer.Deserialize<List<Car>>(json,
          new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (cars == null) return 0;

        // avoid duplicates by LicensePlate
        var existingPlates = _ctx.Cars.Select(c => c.LicensePlate).ToHashSet();

        var toAdd = cars.Where(c => !existingPlates.Contains(c.LicensePlate)).ToList();
        if (toAdd.Count > 0)
        {
            _ctx.Cars.AddRange(toAdd);
            await _ctx.SaveChangesAsync();
        }

        return toAdd.Count;
    }
}
