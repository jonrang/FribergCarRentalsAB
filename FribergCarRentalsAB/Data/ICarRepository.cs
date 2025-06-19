using FribergCarRentalsAB.Models;

namespace FribergCarRentalsAB.Data
{
    public interface ICarRepository
    {
        Task<Car> GetById(int id);
        Task<Car> Find(int id);
        Task<List<Car>> GetAll();
        Task Create(Car car);
        Task Update(Car car);
        Task Delete(int id);
    }
}
