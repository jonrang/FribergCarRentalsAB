using FribergCarRentalsAB.Models;
using Microsoft.EntityFrameworkCore;

namespace FribergCarRentalsAB.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext applicationDb;

        public UserRepository(ApplicationDbContext applicationDbContext)
        {
            this.applicationDb = applicationDbContext;
        }
        public async Task Create(ApplicationUser applicationUser)
        {
            applicationDb.Users.Add(applicationUser);
            await applicationDb.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var user = GetByIdAsync(id);
            if (user != null)
            {
                applicationDb.Remove(user);
                await applicationDb.SaveChangesAsync();
            }
        }

        public async Task<ApplicationUser> Find(int id)
        {
            return await applicationDb.Users.FindAsync(id);
        }


        public async Task<IList<ApplicationUser>> GetAllAsync()
        {
            return await applicationDb.Users.AsNoTracking().ToListAsync();
        }

        public async Task<ApplicationUser> GetByIdAsync(int id)
        {
            return await applicationDb.Users.Include(r => r.Rentals).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task Update(ApplicationUser applicationUser)
        {
            applicationDb.Update(applicationUser);
            await applicationDb.SaveChangesAsync();
        }
    }
}
