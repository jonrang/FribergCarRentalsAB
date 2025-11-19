using System.Security.Claims;
using FribergCarRentalsAPI.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FribergCarRentalsAPI.Data.Services
{
    public class RentalService : IRentalService
    {
        private readonly CarRentalAPIContext context;
        private readonly ICarService carService;
        private readonly IConfiguration configuration;

        public RentalService(CarRentalAPIContext context, ICarService carService, IConfiguration configuration)
        {
            this.context = context;
            this.carService = carService;
            this.configuration = configuration;
        }
        public async Task<(bool Success, string? Error, decimal FinalCost)> CompleteRentalAsync(int rentalId)
        {
            var rental = await context.Rentals
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null)
            {
                return (false, "Rental not found", 0);
            }

            if (rental.Status == RentalStatus.Completed)
            {
                return (false, "Rental is already marked as completed.", rental.TotalCost);
            }

            if (rental.Status != RentalStatus.Active)
            {
                return (false, $"Cannot complete rental with status: {rental.Status}.", 0);
            }

            var actualReturnDateTime = DateTime.UtcNow;

            var lateFeeMultiplier = configuration.GetValue<decimal>("RentalSettings:LateFeeMultiplier", 1.5m);
            var graceHours = configuration.GetValue<double>("RentalSettings:LateFeeGraceHours", 1.0);

            var bookedEndDateTime = rental.EndDate.ToDateTime(new TimeOnly(23, 59, 59)).AddHours(graceHours);

            decimal lateFees = 0.00m;

            if (actualReturnDateTime > bookedEndDateTime)
            {
                var lateDuration = actualReturnDateTime - bookedEndDateTime;

                int lateDays = (int)Math.Ceiling(lateDuration.TotalDays);

                decimal lateFeeRate = rental.RateAtTimeOfRental * lateFeeMultiplier;

                lateFees = lateFeeRate * lateDays;
            }

            rental.ActualReturnDate = actualReturnDateTime;
            rental.Fees = lateFees;
            rental.TotalCost += lateFees;
            rental.Status = RentalStatus.Completed;

            if (rental.Car != null)
            {
                rental.Car.IsAvailable = true;
                // Optionally update mileage here if the data was collected at return
                // rental.Car.Mileage = newMileage;
            }

            await context.SaveChangesAsync();

            return (true, null, rental.TotalCost);
        }

        public async Task<(bool Success, string? Error, RentalViewDto? Rental)> CreateRentalAsync(CreateRentalDto rentalDto, string userId, ClaimsPrincipal userClaims)
        {
            var isOfAge = userClaims.FindFirst("IsOfAge")?.Value == "True";
            var hasLicense = userClaims.FindFirst("HasDriverLicense")?.Value == "True";

            if (!isOfAge || !hasLicense)
            {
                return (false, "User must be of age and have a valid driver's license on file to book.", null);
            }

            var car = await context.Cars.Include(c => c.Model).FirstOrDefaultAsync(c => c.Id == rentalDto.CarId);
            if (car == null || !car.IsAvailable)
            {
                return (false, "Car not found or is currently marked unavailable in inventory.", null);
            }

            var availableCars = await carService.GetAvailableCarsAsync(rentalDto.StartDate, rentalDto.EndDate);
            if (!availableCars.Any(c => c.CarId == rentalDto.CarId))
            {
                return (false, "The selected car is already booked for part or all of the requested period.", null);
            }

            var daysBooked = rentalDto.EndDate.DayNumber - rentalDto.StartDate.DayNumber + 1;
            if (daysBooked <= 0)
            {
                return (false, "Rental duration must be at least one day.", null);
            }

            var totalCost = car.RatePerDay * daysBooked;

            var newRental = new Rental
            {
                CarId = rentalDto.CarId,
                UserId = userId,
                StartDate = rentalDto.StartDate,
                EndDate = rentalDto.EndDate,
                DaysBooked = daysBooked,
                RateAtTimeOfRental = car.RatePerDay,
                TotalCost = totalCost,
                Fees = 0.00m,
                Status = RentalStatus.Pending
            };

            context.Rentals.Add(newRental);
            await context.SaveChangesAsync();

            var rentalView = new RentalViewDto
            {
                RentalId = newRental.Id,
                CarDetails = $"{car.Model.Manufacturer} {car.Model.Name}",
                StartDate = newRental.StartDate,
                EndDate = newRental.EndDate,
                Status = newRental.Status.ToString(),
                TotalCost = newRental.TotalCost,
            };

            return (true, null, rentalView);
        }

        public async Task<(bool Success, string? Error)> ActivateRentalAsync(int rentalId)
        {
            var rental = await context.Rentals
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.Id == rentalId);

            if (rental == null)
            {
                return (false, "Rental not found.");
            }

            if (rental.Status != RentalStatus.Pending)
            {
                return (false, $"Only Pending rentals can be activated. Current status: {rental.Status}.");
            }

            rental.Status = RentalStatus.Active;

            if (rental.Car != null)
            {
                rental.Car.IsAvailable = false;
            }

            await context.SaveChangesAsync();
            return (true, null);
        }

        public Task<IEnumerable<RentalViewDto>> GetAllRentalsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<RentalViewDto?> GetRentalByIdAsync(int rentalId)
        {
            var rental = await context.Rentals
                .Where(r => r.Id == rentalId)
                .Include(r => r.Car!)
                    .ThenInclude(c => c.Model)
                .Include(r => r.User)
                .Select(r => new RentalViewDto
                {
                    RentalId = r.Id,
                    CarDetails = $"{r.Car!.Model.Manufacturer} {r.Car.Model.Name} ({r.Car.LicensePlate})",
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Status = r.Status.ToString(),
                    TotalCost = r.TotalCost,
                    UserId = r.UserId,
                    UserEmail = r.User.Email!
                })
                .FirstOrDefaultAsync();
            
            return rental;
        }

        public async Task<IEnumerable<RentalViewDto>> GetUserRentalsAsync(string userId)
        {
            var rentals = await context.Rentals
                .Where(r => r.UserId == userId)
                .Include(r => r.Car!)
                    .ThenInclude(c => c.Model)
                .Include(r => r.User)
                .OrderByDescending(r => r.StartDate)
                .Select(r => new RentalViewDto
                {
                    RentalId = r.Id,
                    CarDetails = $"{r.Car!.Model.Manufacturer} {r.Car.Model.Name} ({r.Car.LicensePlate})",
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Status = r.Status.ToString(),
                    TotalCost = r.TotalCost,
                    UserId = r.UserId,
                    UserEmail = r.User.Email!
                })
                .ToListAsync();

            return rentals;
        }

        public Task<(bool Success, string? Error)> UpdateRentalStatusAsync(int rentalId, RentalStatus newStatus)
        {
            throw new NotImplementedException();
        }
        public async Task<(bool Success, string? Error)> CancelRentalAsync(int rentalId, string userId)
        {
            var rental = await context.Rentals.FindAsync(rentalId);

            if (rental == null)
            {
                return (false, "Rental not found.");
            }

            if (rental.UserId != userId)
            {
                return (false, "Not authorized to cancel this rental.");
            }
            if (rental.Status != RentalStatus.Pending)
            {
                return (false, $"Cancellation is only allowed for Pending rentals. Current status: {rental.Status}.");
            }

            var cutoffHours = configuration.GetValue<int>("RentalSettings:CancellationCutoffHours");


            var rentalStartTime = rental.StartDate.ToDateTime(new TimeOnly(0, 0, 0));
            var cancellationDeadline = rentalStartTime.AddHours(-cutoffHours);

            if (DateTime.UtcNow > cancellationDeadline)
            {
                return (false, $"Cancellation deadline passed. Rentals must be cancelled at least {cutoffHours} hours before the start date.");
            }

            rental.Status = RentalStatus.Cancelled;

            // Optionally, update the car's IsAvailable status
            // However, the GetAvailableCarsAsync method already handles this via Status check.

            await context.SaveChangesAsync();
            return (true, null);
        }
    }
}
