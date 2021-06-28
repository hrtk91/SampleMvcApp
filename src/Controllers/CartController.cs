using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SampleMvcApp.Data;
using SampleMvcApp.Models;

namespace SampleMvcApp.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly SampleMVCAppContext _context;
        private UserManager<IdentityUser> _userManager;

        public CartController(SampleMVCAppContext context, UserManager<IdentityUser> userManager) : base()
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cart.Include(x => x.Owner).ToListAsync());
        }

        // GET: Cart/Details/5
        public async Task<IActionResult> Details()
        {
            var user = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (user == null)
            {
                return NotFound();
            }

            var cart = await GetOrCreateCart(user.Id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // GET: Cart/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Cart.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            return View(cart);
        }

        // POST: Cart/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CartId")] Cart cart)
        {
            if (id != cart.CartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.CartId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await GetOrCreateCart(user.Id);
            if (cart == null) return NotFound();

            var product = await _context.Product
                .Include(x => x.Shop)
                .ThenInclude(x => x.Owner)
                .FirstOrDefaultAsync(x => x.ProductId == productId);
            if (product == null) return NotFound();
            if (product.Shop.Owner.Id == user.Id) return NotFound();

            cart.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            var user = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var cart = await _context.Cart.Include(x => x.Products).FirstOrDefaultAsync(x => x.Owner.Id == user.Id);
            var target = cart.Products.FirstOrDefault(x => x.ProductId == productId);
            if (target == null)
            {
                return NotFound();
            }
            
            cart.Products.Remove(target);
            await _context.SaveChangesAsync();

            return View(nameof(Details), cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy()
        {
            var user = await GetCurrentUser();
            var cart = await _context.Cart
                .Include(x => x.Owner)
                .Include(x => x.Products)
                .ThenInclude(x => x.Shop)
                .ThenInclude(x => x.Owner)
                .FirstOrDefaultAsync(x => x.Owner.Id == user.Id);
            if (user == null || cart == null) return NotFound();

            var receipts = new List<Receipt>();
            foreach (var product in cart.Products)
            {
                var receipt = new Receipt();
                receipt.Product = product;
                receipt.Price = product.Price;
                receipt.Seller = product.Shop.Owner;
                receipt.Discount = product.Discount;
                receipt.Buyer = user;
                receipt.Timestamp = DateTime.Now;
                receipts.Add(receipt);
            }

            await _context.Receipt.AddRangeAsync(receipts);
            cart.Products.Clear();
            await _context.SaveChangesAsync();

            return View("BuyComplete", receipts);
        }

        private bool CartExists(int id)
        {
            return _context.Cart.Any(e => e.CartId == id);
        }

        private async Task<Cart> GetOrCreateCart(string userId)
        {
            var cart = await _context.Cart.Include(x => x.Products).Include(x => x.Owner).FirstOrDefaultAsync(x => x.Owner.Id == userId);
            if (cart == null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return null;

                cart = new Cart() { Owner = user };
                cart.Products = new List<Product>();
                await _context.Cart.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private async Task<IdentityUser> GetCurrentUser()
        {
            return await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }
    }
}
