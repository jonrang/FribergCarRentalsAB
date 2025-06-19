using System.Threading.Tasks;
using FribergCarRentalsAB.Models;
using Microsoft.EntityFrameworkCore;

namespace FribergCarRentalsAB.Data
{
    public class CarRepository : ICarRepository
    {
        private readonly ApplicationDbContext applicationDb;

        public CarRepository(ApplicationDbContext applicationDb)
        {
            this.applicationDb = applicationDb;
        }

        public async Task Create(Car car)
        {
            applicationDb.Add(car);
            await applicationDb.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var car = await GetById(id);
            if (car != null)
            {
                try
                {
                    applicationDb.Remove(car);
                    await applicationDb.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public async Task<Car> Find(int id)
        {
            try
            {
                return await applicationDb.Cars.FindAsync(id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Car> GetById(int id)
        {
            try
            {
                return await applicationDb.Cars.Include(r => r.Rentals).FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<List<Car>> GetAll()
        {
            return await applicationDb.Cars.ToListAsync();
        }


        public async Task Update(Car car)
        {
            try
            {
                applicationDb.Update(car);
                await applicationDb.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }
    }
}
