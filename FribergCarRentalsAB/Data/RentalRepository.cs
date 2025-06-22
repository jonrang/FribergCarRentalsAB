using FribergCarRentalsAB.Models;
using Microsoft.EntityFrameworkCore;

namespace FribergCarRentalsAB.Data
{
    public class RentalRepository : IRentalRepository
    {
        private readonly ApplicationDbContext applicationDb;

        public RentalRepository(ApplicationDbContext applicationDb)
        {
            this.applicationDb = applicationDb;
        }
        public async Task Create(Rental rental)
        {
            applicationDb.Rentals.Add(rental);
            await applicationDb.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var rental = await applicationDb.Rentals.FindAsync(id);
            if (rental != null)
            {
                applicationDb.Rentals.Remove(rental);
                await applicationDb.SaveChangesAsync();
            }
        }

        public async Task<IList<Rental>> GetAllAsync()
        {
            return await applicationDb.Rentals.Include(u => u.User).Include(c => c.Car).ToListAsync();
        }

        public async Task<Rental> GetByIdAsync(int id)
        {
            return await applicationDb.Rentals.Include(u => u.User).Include(c => c.Car).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task Update(Rental rental)
        {
            applicationDb.Rentals.Update(rental);
            await applicationDb.SaveChangesAsync();
        }

        public async Task<List<Rental>> FindOverlappingAsync(int carId, DateTime start, DateTime end)
        {
            return await applicationDb.Rentals
              .Where(r =>
                 r.CarId == carId &&
                 (r.Status == RentalStatus.Pending || r.Status == RentalStatus.Active) &&
                 r.StartDate < end &&
                 (r.ActualReturnDate ?? r.EndDate) > start
              )
              .ToListAsync();
        }

    }
}
