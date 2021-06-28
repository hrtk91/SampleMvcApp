using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleMvcApp.ViewModels.User;

namespace SampleMvcApp.Controllers
{
    public class UserController : Controller
    {
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private SignInManager<IdentityUser> _signInManager;

        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signinManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signinManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string UserName, string Email, string Password)
        {
            if (string.IsNullOrEmpty(UserName)) return NotFound();

            var user = new IdentityUser(UserName)
            {
                Email = Email
            };

            var createResult = await _userManager.CreateAsync(user, Password);
            if (createResult.Succeeded && TryValidateModel(user))
            {
                return RedirectToAction(nameof(Index), "Product");
            }

            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return View();
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = (await _userManager.GetRolesAsync(user)).ToList();
            var roles = await _roleManager.Roles.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();

            var vm = new ViewModels.User.EditViewModel(user, userRoles, roles);

            return View(vm);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit([Bind("Id, Email")] IdentityUser user, IEnumerable<string> roles)
        {
            var requester = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var updateUser = await _userManager.FindByIdAsync(user.Id);

            if (!(await _userManager.IsInRoleAsync(requester, "Admin")))
            {
                if (updateUser == null || requester.Id != updateUser.Id)
                {
                    return NotFound();
                }
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


        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel() { ReturnUrl = GetReturnUrl(returnUrl) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("UserName", "Password", "ReturnUrl")]LoginViewModel vm)
        {
            // 第三引数は仮でfalse(ログイン記憶をオフ)
            var result = await _signInManager.PasswordSignInAsync(vm.UserName, vm.Password, false, false);
            if (result.Succeeded)
            {
                return LocalRedirect(GetReturnUrl(vm.ReturnUrl));
            }

            ModelState.AddModelError("", "login failed attempt.");
            return View(nameof(Login), vm);
        }

        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            if (!_signInManager.IsSignedIn(User)) return View();

            await _signInManager.SignOutAsync();

            return LocalRedirect(GetReturnUrl(returnUrl));
        }

        private string GetReturnUrl(string returnUrl = null)
            => returnUrl ?? Url.Action(nameof(ProductController.Index), "Product");
    }
}