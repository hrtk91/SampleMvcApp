using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SampleMvcApp.Controllers
{
    public class ControllerBase : Controller
    {
        protected UserManager<IdentityUser> _userManager;
        
        public ControllerBase(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        protected async Task<IdentityUser> GetCurrentUser()
        {
            return await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }
    }
}