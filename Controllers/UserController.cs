using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SampleMvcApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = (await _userManager.GetRolesAsync(user)).DefaultIfEmpty().ToList();
            var roles = await _roleManager.Roles.Select(x => x.Name).ToListAsync();

            var vm = new ViewModels.User.EditViewModel(user, userRoles, roles);

            return View(vm);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit([Bind("Id, Email")] IdentityUser user, IEnumerable<string> roles)
        {
            var updateUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (updateUser == null)
            {
                return NotFound();
            }

            var resultUpdateUser = await TryUpdateModelAsync<IdentityUser>(updateUser, "User", x => x.Email);
            var resultRemoveRole = await _userManager.RemoveFromRolesAsync(updateUser, await _userManager.GetRolesAsync(updateUser));
            var resultUpdateRole = await _userManager.AddToRolesAsync(updateUser, roles);
            if (resultUpdateRole.Succeeded && resultRemoveRole.Succeeded && resultUpdateUser)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return UnprocessableEntity();
            }
        }
    }
}