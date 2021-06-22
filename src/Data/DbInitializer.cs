using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SampleMvcApp.Data
{
    public static class DbInitializer
    {
        public static string UserName { get; } = "admin@mail.com";
        public static async Task CreateAdministrator(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var name = UserName;
            var adminUser = await userManager.FindByNameAsync(name);
            if (adminUser == null)
            {
                var createResult = await userManager.CreateAsync(
                    new IdentityUser(name) { Email = name, NormalizedEmail = name.ToUpper(), NormalizedUserName = name.ToUpper() },
                    "Abc123#");
                if (!createResult.Succeeded) return;

                adminUser = await userManager.FindByNameAsync(name);
                if (adminUser == null) return;
            }

            if (await roleManager.RoleExistsAsync("Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}