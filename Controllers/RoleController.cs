using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SampleMvcApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View(_roleManager.Roles.ToList());
        }

        public IActionResult Create()
        {
            return View();            
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name")] string name)
        {
            var roleExists = await _roleManager.RoleExistsAsync(name);
            if (roleExists) Conflict();

            var result = await _roleManager.CreateAsync(new IdentityRole(name));
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return UnprocessableEntity();
            }
        }

        public async Task<IActionResult> Delete(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByNameAsync(name);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([Bind("Name")] string name)
        {
            var role = await _roleManager.FindByNameAsync(name);
            if (role == null)
            {
                return NotFound();
            }

            await _roleManager.DeleteAsync(role);
            return RedirectToAction(nameof(Index));
        }
    }
}