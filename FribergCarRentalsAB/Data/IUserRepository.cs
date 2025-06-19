using FribergCarRentalsAB.Models;

namespace FribergCarRentalsAB.Data
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetByIdAsync(int id);
        Task<ApplicationUser> Find(int id);
        Task<IList<ApplicationUser>> GetAllAsync();
        Task Create(ApplicationUser applicationUser);
        Task Update(ApplicationUser applicationUser);
        Task Delete(int id);
    }
}
