using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleMvcApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SampleMvcApp.ViewModels.Product;
using SampleMvcApp.Services.Interface;

namespace SampleMvcApp.Controllers
{
    public class ProductController : Controller
    {
        private ILogger<ProductController> logger;

        private readonly IProductService productService;

        public ProductController(ILogger<ProductController> logger, IProductService productService)
        {
            this.logger = logger;
            this.productService = productService;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            return View(await productService.Index());
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
        public async Task<IActionResult> Create([Bind("ProductId,Name,Description,Price")] Product product, int shopId)
        {
            try
            {
                await productService.PostCreate(product, shopId, User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
            catch (ArgumentException ex)
            {
                logger.LogTrace(ex, "ショップorユーザーの入力エラー");
                ModelState.AddModelError(ex.ParamName, ex.Message);
            }

            if (!ModelState.IsValid) return View(product);

            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                return View(await productService.Detail(id));
            }
            catch (ArgumentNullException ex)
            {
                // idの商品が見つからない場合はNofFound
                logger.LogTrace(ex, "該当商品の取得に失敗しました");
                return NotFound();
            }
        }

        // GET: Product/Edit/5
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                return View(await productService.Edit(id));
            }
            catch (ArgumentException ex)
            {
                // idの商品が見つからない場合はNofFound
                logger.LogTrace(ex, "該当商品の取得に失敗しました");
                return NotFound();
            }
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Edit(int id,
            [Bind("ProductId,Name,Price,Description,Discount")] Product product,
            [Bind("Genres")] IEnumerable<int> genres,
            [Bind("Files")] IEnumerable<IFormFile> files)
        {
            /// ファイル内容チェック
            var validationResults = productService.ValidateProductImageFiles(files);
            foreach (var result in validationResults)
            {
                ModelState.AddModelError(string.Empty, $"{result.FileName} : {result.Message}");
            }

            if (!ModelState.IsValid)
            {
                var allGenres = await productService.GetSortedAllGenres();
                return View(new EditViewModel(product, allGenres, genres));
            }

            try
            {
                await productService.EditPost(product, genres, files);
            }
            catch (ArgumentException ex)
            {
                // idの商品が見つからない場合はNofFound
                logger.LogTrace(ex, "該当商品の取得に失敗しました");
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Delete/5
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                return View(await productService.Delete(id));
            }
            catch (ArgumentException ex)
            {
                // idの商品が見つからない場合はNofFound
                logger.LogTrace(ex, "該当商品の取得に失敗しました");
                return NotFound();
            }
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await productService.DeleteConfirmed(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
