using LearniVerseNew.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LearniVerseNew
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            CreateAdminUserAndRoles();
        }

        private void CreateAdminUserAndRoles()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Create roles if they don't exist
            var roles = new[] { "Admin", "User", "Teacher" }; // Add more roles if needed
            foreach (var role in roles)
            {
                if (!roleManager.RoleExists(role))
                {
                    var roleResult = roleManager.Create(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception($"Failed to create role: {role}");
                    }
                }
            }

            // Create admin user if it doesn't exist
            var adminEmail = "Admin@admin.com"; // Change as needed
            var adminUser = userManager.FindByEmail(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };
                var result = userManager.Create(adminUser, "Admin1234."); // Change the password as needed
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create admin user: {result.Errors}");
                }

                // Assign admin role to the admin user
                result = userManager.AddToRole(adminUser.Id, "Admin");
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to assign role to admin user: {result.Errors}");
                }
            }
        }
    }
}
