using FribergCarRentalsAB.Models;

namespace FribergCarRentalsAB.Data
{
    public interface IRentalRepository
    {
        Task<Rental> GetByIdAsync(int id);
        Task<IList<Rental>> GetAllAsync();
        Task Create(Rental rental);
        Task Update(Rental rental);
        Task Delete(int id);
        Task<List<Rental>> FindOverlappingAsync(int carId, DateTime start, DateTime end);

    }
}
