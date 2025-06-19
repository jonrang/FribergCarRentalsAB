using FribergCarRentalsAB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FribergCarRentalsAB.Data
{
    public class RentalService : IRentalService
    {
        private readonly ICarRepository cars;
        private readonly IRentalRepository rentals;
        private readonly IUserRepository users;
        private readonly ApplicationDbContext db; // for transaction/SaveChanges

        public RentalService(
          ICarRepository carRepo,
          IRentalRepository rentalRepo,
          IUserRepository userManager,
          ApplicationDbContext dbContext)
        {
            cars = carRepo;
            rentals = rentalRepo;
            users = userManager;
            db = dbContext;
        }

        public async Task<bool> IsCarAvailableAsync(int carId, DateTime start, DateTime end)
        {
            // find any overlapping rentals
            var conflicts = await rentals
              .FindOverlappingAsync(carId, start, end);
            return !conflicts.Any();
        }

        public async Task<Rental> CreateRentalAsync(int userId, int carId, DateTime start, DateTime end)
        {
            if (!await IsCarAvailableAsync(carId, start, end))
                throw new InvalidOperationException("Car is not available in that period.");

            var car = await cars.GetById(carId);
            if (car == null) throw new ArgumentException("Invalid car", nameof(carId));
            RentalStatus status;
            if (start <= DateTime.Now)
            {
                status = RentalStatus.Active;
            }
            else
            {
                status = RentalStatus.Pending;
            }
            var rental = new Rental
            {
                UserId = userId,
                CarId = carId,
                StartDate = start,
                EndDate = end,
                Status = status
            };

            // transactionally create rental + flip availability
            await db.Database.BeginTransactionAsync();
            try
            {
                await rentals.Create(rental);
                car.IsAvailable = false;
                await cars.Update(car);
                await db.Database.CommitTransactionAsync();
            }
            catch
            {
                await db.Database.RollbackTransactionAsync();
                throw;
            }

            return rental;
        }

        public async Task<decimal> ComputeLateFeeAsync(int rentalId)
        {
            var rental = await rentals.GetByIdAsync(rentalId);
            if (rental == null) throw new ArgumentException("Not found", nameof(rentalId));
            if (rental.ActualReturnDate == null || rental.ActualReturnDate <= rental.EndDate)
                return 0m;

            var daysLate = (rental.ActualReturnDate.Value - rental.EndDate).Days;
            const decimal LateRateMultiplier = 1.5m;
            return daysLate * rental.Car.RatePerDay * LateRateMultiplier;
        }

        public async Task<Rental> MarkReturnedAsync(int rentalId)
        {
            var rental = await rentals.GetByIdAsync(rentalId);
            if (rental == null) throw new ArgumentException("Not found", nameof(rentalId));
            if (rental.Status == RentalStatus.Completed)
                return rental;

            rental.ActualReturnDate = DateTime.UtcNow;
            rental.Status = RentalStatus.Completed;

            // recalc total cost
            var days = (rental.ActualReturnDate.Value - rental.StartDate).Days;
            var baseCost = days * rental.Car.RatePerDay;
            var lateFee = await ComputeLateFeeAsync(rentalId);
            rental.TotalCost = baseCost + lateFee;

            // flip car availability
            var car = await cars.GetById(rental.CarId);
            car.IsAvailable = true;

            // save both
            await db.Database.BeginTransactionAsync();
            try
            {
                await rentals.Update(rental);
                await cars.Update(car);
                await db.Database.CommitTransactionAsync();
            }
            catch
            {
                await db.Database.RollbackTransactionAsync();
                throw;
            }

            return rental;
        }

        public async Task<SelectList> GetAvailableCarsAsync(int? carId, DateTime? start = null, DateTime? end = null)
        {
            var all = await cars.GetAll();
            var list = all
              .Where(c => c.IsAvailable
                && (start == null || end == null ||
                    (c.Rentals.All(r => r.EndDate < start || r.StartDate > end))))
              .Select(c => new { c.Id, Name = $"{c.Make} {c.Model} ({c.Year})" })
              .ToList();
            return new SelectList(list, "Id", "Name", carId);
        }

        public async Task<SelectList> GetUsersSelectListAsync()
        {
            var users = await this.users.GetAllAsync();
            var list =  users.Select((ApplicationUser u) => new { u.Id, Display = u.FullName ?? u.UserName })
              .ToList();
            return new SelectList(list, "Id", "Display");
        }

        public async Task<IList<Rental>> GetActiveRentalsAsync(int userId = 0)
        {
            var query = db.Rentals.AsQueryable().Where(r => r.Status == RentalStatus.Active || r.Status == RentalStatus.Pending );
            if (0!=userId)
                query = query.Where(r => r.UserId == userId);
            return await query.Include(r => r.Car).Include(r => r.User).ToListAsync();
        }

        public async Task<IList<Rental>> GetOldRentalsAsync(int userId = 0)
        {
            var query = db.Rentals.AsQueryable().Where(r => r.Status == RentalStatus.Completed || r.Status == RentalStatus.Cancelled);
            if (0 != userId)
                query = query.Where(r => r.UserId == userId);
            return await query.Include(r => r.Car).Include(r => r.User).ToListAsync();
        }

        public async Task CancelRentalAsync(int rentalId)
        {
            var rental = await rentals.GetByIdAsync(rentalId);
            if (rental == null) return;
            if (rental.Status != RentalStatus.Pending)
                throw new InvalidOperationException("Can only cancel pending reservations.");

            rental.Status = RentalStatus.Cancelled;
            var car = await cars.GetById(rental.CarId);
            car.IsAvailable = true;

            await db.Database.BeginTransactionAsync();
            try
            {
                await rentals.Update(rental);
                await cars.Update(car);
                await db.Database.CommitTransactionAsync();
            }
            catch
            {
                await db.Database.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
