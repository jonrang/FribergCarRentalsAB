using Blazored.LocalStorage;
using FribergCarRentalsABClient;
using FribergCarRentalsABClient.Providers;
using FribergCarRentalsABClient.Services.Admin;
using FribergCarRentalsABClient.Services.Authentication;
using FribergCarRentalsABClient.Services.Base;
using FribergCarRentalsABClient.Services.Cars;
using FribergCarRentalsABClient.Services.Rental;
using FribergCarRentalsABClient.Services.Users;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace FribergCarRentalsABClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddLogging();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
                sp.GetRequiredService<ApiAuthenticationStateProvider>());

            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<ITokenRefresher, AuthenticationService>();
            builder.Services.AddScoped<ICarService, CarService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IRentalService, RentalService>();

            var apiUrl = builder.Configuration["ApiSettings:BaseUrl"]
             ?? builder.HostEnvironment.BaseAddress;


            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(apiUrl)
            });

            builder.Services.AddScoped<ICarRentalsAPIClient, CarRentalsAPIClient>(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();

                var localStorage = sp.GetRequiredService<ILocalStorageService>();
                var serviceProvider = sp.GetRequiredService<IServiceProvider>();

                return new CarRentalsAPIClient(
                    httpClient,
                    localStorage,
                    serviceProvider
                );
            });


            await builder.Build().RunAsync();
        }
    }
}
