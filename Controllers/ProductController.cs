using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SampleMvcApp.Models;
using SampleMvcApp.ViewModels;

namespace SampleMvcApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly SampleMVCAppContext _context;

        public ProductController(SampleMVCAppContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            return View(await _context.Product.ToListAsync());
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ProductId,Name,Price")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductGenres).ThenInclude(pg => pg.Genre).FirstOrDefaultAsync(p => p.ProductId == id);
            var genres = await _context.Genre.ToListAsync();
            if (product == null || genres == null)
            {
                return NotFound();
            }

            var vm = new ViewModels.Product.EditViewModel(product, genres, product.ProductGenres.Select(x => x.GenreId));

            return View(vm);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("ProductId,Name,Price,Description,Discount,ProductGenres")] Product product,
            [Bind("Genres")] IEnumerable<int> genres)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var productToUpdate = await _context.Product.Include(p => p.ProductGenres).FirstOrDefaultAsync(p => p.ProductId == id);

                if (await TryUpdateModelAsync(productToUpdate, "product", x => x.Description, x => x.Discount, x => x.Name, x => x.Price))
                {
                    var beforeGenres = productToUpdate.ProductGenres.Select(pg => pg.GenreId);
                    var afterGenres = genres;
                    var addGenreIds = afterGenres.Except(beforeGenres);
                    var removeGenreIds = beforeGenres.Except(afterGenres);

                    foreach (var genreId in addGenreIds)
                    {
                        productToUpdate.ProductGenres.Add(new ProductGenre() { ProductId = productToUpdate.ProductId, GenreId = genreId });
                    }

                    foreach (var genreId in removeGenreIds)
                    {
                        var remove = productToUpdate.ProductGenres.FirstOrDefault(pg => pg.GenreId == genreId);
                        _context.Remove(remove);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProductExists(product.ProductId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var vm = new ViewModels.Product.EditViewModel(product, await _context.Genre.ToListAsync(), genres);
            return View(vm);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}
