using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles =
            {
            RoleNames.Admin,
            RoleNames.User
        };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var adminEmail = configuration["Admin:Email"];

            if (string.IsNullOrWhiteSpace(adminEmail))
                return;

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
                return;

            // Remove User role if exists
            if (await userManager.IsInRoleAsync(adminUser, RoleNames.User))
            {
                await userManager.RemoveFromRoleAsync(
                    adminUser,
                    RoleNames.User);
            }

            // Add Admin role if not exists
            if (!await userManager.IsInRoleAsync(adminUser, RoleNames.Admin))
            {
                await userManager.AddToRoleAsync(
                    adminUser,
                    RoleNames.Admin);
            }
        }
    }
}
