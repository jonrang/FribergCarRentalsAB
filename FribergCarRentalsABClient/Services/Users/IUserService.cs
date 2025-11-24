using FribergCarRentalsABClient.Services.Base;

namespace FribergCarRentalsABClient.Services.Users
{
    public interface IUserService
    {
        Task<bool> ChangeMyPasswordAsync(ChangePasswordDto changeDto);
        Task<CustomerProfileViewDto?> GetMyProfileAsync();
        Task<bool> UpdateMyProfileAsync(CustomerProfileUpdateDto updateDto);
    }
}