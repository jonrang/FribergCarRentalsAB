using System.Data;
using System.Threading.Tasks;
using FribergCarRentalsAB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace FribergCarRentalsAB.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> users;
        private readonly RoleManager<IdentityRole<int>> roles;

        public AdminController(UserManager<ApplicationUser> users , RoleManager<IdentityRole<int>> roles)
        {
            this.users = users;
            this.roles = roles;
        }
        public async Task<IActionResult> Index()
        {
            // Build the absolute URL to the Profile page:
            var manageUrl = Url.Page(
              "/Account/Manage/Index",
              pageHandler: null,
              new { area = "Identity" });

            if (User.Identity?.IsAuthenticated == true)
            {
                // Logged in? Send straight to Manage
                return LocalRedirect(manageUrl);
            }
            else
            {
                // Not logged in? Send to Login with a returnUrl back to Manage
                var loginUrl = Url.Page(
                  "/Account/Login",
                  pageHandler: null,
                  new { area = "Identity", returnUrl = manageUrl });
                return Redirect(loginUrl);
            }
        }

    }
}


