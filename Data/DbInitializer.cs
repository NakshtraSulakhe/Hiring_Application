using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public static class DbInitializer
{
    public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        string[] roleNames = { "Admin", "HR", "Applicant" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        if (userManager.FindByEmailAsync("admin@hiringapp.com").Result == null)
        {
            IdentityUser adminUser = new IdentityUser
            {
                UserName = "Admin",
                Email = "admin@hiringapp.com",
                EmailConfirmed = true
            };

            IdentityResult result = await userManager.CreateAsync(adminUser, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        if (userManager.FindByEmailAsync("hr@hiringapp.com").Result == null)
        {
            IdentityUser hrUser = new IdentityUser
            {
                UserName = "hr@hiringapp.com",
                Email = "hr@hiringapp.com",
                EmailConfirmed = true
            };

            IdentityResult result = await userManager.CreateAsync(hrUser, "Hr@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(hrUser, "HR");
            }
        }
    }
}
