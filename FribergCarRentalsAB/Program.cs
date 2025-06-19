using FribergCarRentalsAB.Data;
using FribergCarRentalsAB.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace FribergCarRentalsAB
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // If error occurs, make sure there is no extra Identity service.
            // When building project scaffolding might add a
            // builder.Services.AddDefaultIdentity...
            // It is safe to remove
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                            .AddEntityFrameworkStores<ApplicationDbContext>()
                            .AddDefaultUI()
                            .AddDefaultTokenProviders();

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddTransient<ICarRepository, CarRepository>();
            builder.Services.AddTransient<IUserRepository, UserRepository>();
            builder.Services.AddTransient<IRentalRepository, RentalRepository>();
            builder.Services.AddScoped<IRentalService, RentalService>();
            builder.Services.AddTransient<IDbSeeder, DbSeeder>();



            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await IdentitySeeder.SeedRolesAndAdminAsync(services);
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapDefaultControllerRoute();

            //app.MapControllerRoute(
            //    name: "default",
            //    pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
//-Validation attributes(e.g.ensure EndDate > StartDate)
//- A small lookup table for car categories (SUV, Sedan, etc.)
//- Soft-delete flags or audit trails
//- A simple availability checker that filters out cars with overlapping active rentals
//That blueprint should give you a fully working toy rental site with clear separation of concerns�and a foundation you can easily extend.
