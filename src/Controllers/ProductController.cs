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
using System.Security.Claims;
using SampleMvcApp.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Logging;
using SampleMvcApp.Services;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using SampleMvcApp.ViewModels.Product;

namespace SampleMvcApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly SampleMVCAppContext _context;
        private ILogger<ProductController> _logger;
        private IProductImageService _PIS;

        public ProductController(
            SampleMVCAppContext context, ILogger<ProductController> logger,
            IProductImageService productImageService)
        {
            _context = context;
            _logger = logger;
            _PIS = productImageService;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            return View(await _context.Product.Include(x => x.Shop).ThenInclude(x => x.Owner).ToListAsync());
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(x => x.ProductImages)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        [Authorize(Roles = "Seller")]
        public IActionResult Create(int shopId)
        {
            var vm = new ViewModels.Product.CreateViewModel(shopId);
            return View(vm);
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Create([Bind("ProductId,Name,Price")] Product product, int shopId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var shop = await _context.Shop
                .Include(x => x.Owner)
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.ShopId == shopId);
            if (shop == null)
            {
                return NotFound();
            }

            if (shop.Owner.Id != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                shop.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Product/Edit/5
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductGenres).ThenInclude(pg => pg.Genre).FirstOrDefaultAsync(p => p.ProductId == id);
            var genres = await _context.Genre.OrderBy(x => x.Name).ToListAsync();
            var productImages = await _context.ProductImages.Where(x => x.Product.ProductId == product.ProductId).ToListAsync();
            if (product == null || genres == null)
            {
                return NotFound();
            }

            var vm = new ViewModels.Product.EditViewModel(product, genres, product.ProductGenres.Select(x => x.GenreId), productImages);

            return View(vm);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(int id,
            [Bind("ProductId,Name,Price,Description,Discount,ProductGenres")] Product product,
            [Bind("Genres")] IEnumerable<int> genres,
            [Bind("Files")] IEnumerable<IFormFile> files)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            /// ファイル内容チェック
            foreach (var file in files)
            {
                if (!(file.Length > 0))
                {
                    _logger.LogWarning("0 bytes file: {0}", file.FileName);
                    ModelState.AddModelError("", $"{file.FileName} is 0 bytes file.");
                    continue;
                }

                if (!_PIS.IsJpeg(file) && !_PIS.IsPng(file))
                {
                    _logger.LogWarning("Not jpeg or png: {0}", file.FileName);
                    ModelState.AddModelError("", $"{file.FileName} is not jpeg or png.");
                    continue;
                }
            }

            if (!ModelState.IsValid)
            {
                return View(new EditViewModel(product, await _context.Genre.OrderBy(x => x.Name).ToListAsync(), genres));
            }

            var productToUpdate = await _context.Product.Include(p => p.ProductGenres).FirstOrDefaultAsync(p => p.ProductId == id);

            if (!await TryUpdateModelAsync(productToUpdate, nameof(Product), x => x.Description, x => x.Discount, x => x.Name, x => x.Price))
            {
                return NotFound();
            }

            var beforeGenres = productToUpdate.ProductGenres.Select(pg => pg.GenreId);
            var afterGenres = genres;
            var addGenreIds = afterGenres.Except(beforeGenres);
            var removeGenreIds = beforeGenres.Except(afterGenres);

            foreach (var genreId in addGenreIds)
            {
                productToUpdate.ProductGenres.Add(
                    new ProductGenre() { ProductId = productToUpdate.ProductId, GenreId = genreId });
            }

            foreach (var genreId in removeGenreIds)
            {
                var removeGenre = productToUpdate.ProductGenres.First(pg => pg.GenreId == genreId);
                _context.Remove(removeGenre);
            }

            foreach (var file in files)
            {
                var filename = _PIS.GetGuidFileName(Path.GetExtension(file.FileName));
                var accessPath = await _PIS.SaveUploadFile(file, filename);

                var productImage = new ProductImage()
                {
                    Uri = accessPath,
                    Timestamp = DateTime.Now,
                    Product = productToUpdate,
                };
                await _context.ProductImages.AddAsync(productImage);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

        // GET: Product/Delete/5
        [Authorize(Roles = "Seller")]
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
        [Authorize(Roles = "Seller")]
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
