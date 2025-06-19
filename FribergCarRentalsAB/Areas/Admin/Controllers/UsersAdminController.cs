using FribergCarRentalsAB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FribergCarRentalsAB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class UsersAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole<int>> roleManager;

        public UsersAdminController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var users = await userManager.Users.ToListAsync();
            var vm = new List<UserAdminViewModel>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                vm.Add(new UserAdminViewModel {
                    Id = user.Id,
                    Email = user.Email!,
                    Roles = roles
                });
            }
            return View(vm);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();
            var allRoles = roleManager.Roles.Select(r => r.Name!).ToList();
            var userRoles = await userManager.GetRolesAsync(user);
            var vm = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                AllRoles = allRoles,
                SelectedRoles = userRoles
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel vm)
        {
            var user = await userManager.FindByIdAsync(vm.Id.ToString());
            if (user == null) return NotFound();

            
            user.FullName = vm.FullName;
            var upd = await userManager.UpdateAsync(user);
            if (!upd.Succeeded)
            {
                ModelState.AddModelError("", "Could not update user.");
                // repopulate AllRoles & SelectedRoles then return View(vm)
            }


            var current = await userManager.GetRolesAsync(user);
            var toAdd = vm.SelectedRoles.Except(current);
            var toRemove = current.Except(vm.SelectedRoles);

            await userManager.AddToRolesAsync(user, toAdd);
            await userManager.RemoveFromRolesAsync(user, toRemove);

            TempData["Success"] = "Användare uppdaterad.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user != null)
            { 
                await userManager.DeleteAsync(user);
                TempData["Success"] = "Användare borttagen.";
            }
            return RedirectToAction(nameof(Index));
        }


    }
}
