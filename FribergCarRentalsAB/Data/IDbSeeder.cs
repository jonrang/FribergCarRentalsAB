namespace FribergCarRentalsAB.Data
{
    public interface IDbSeeder
    {
        Task<int> SeedCarsFromJsonAsync();
    }

}
