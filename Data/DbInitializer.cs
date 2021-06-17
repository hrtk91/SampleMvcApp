using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SampleMvcApp.Data
{
    public static class DbInitializer
    {
        public static string UserName { get; } = "admin@mail.com";
        public static async Task CreateAdministrator(UserManager<IdentityUser> userManager)
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

            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}