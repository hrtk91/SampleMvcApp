using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SampleMvcApp.Data;
using SampleMvcApp.Models;
using SampleMvcApp.Services.Interface;

namespace SampleMvcApp.Services
{
    public class ProductService : IProductService
    {
        private readonly SampleMVCAppContext context;
        private ILogger<ProductService> logger;
        private IProductImageService PIS;

        public ProductService(SampleMVCAppContext context, ILogger<ProductService> logger, IProductImageService pis)
        {
            this.context = context;
            this.logger = logger;
            this.PIS = pis;
        }

        public async Task<IEnumerable<Product>> Index(
            int page = 1,
            int take = 20,
            string sortBy = nameof(Product.ProductId),
            bool orderByDesc = false)
        {
            try
            {
                logger.LogInformation($"商品一覧取得サービス処理 : 開始");

                var list = context.Product
                        .Include(x => x.ProductImages)
                        .Include(x => x.Genres)
                        .Include(x => x.Shop)
                            .ThenInclude(x => x.Owner)
                        .AsSplitQuery();
                var ordered = sortBy switch
                {
                    nameof(Product.Name) =>
                        orderByDesc ? list.OrderByDescending(x => x.Name) : list.OrderBy(x => x.Name),
                    nameof(Product.Price) =>
                        orderByDesc ? list.OrderByDescending(x => x.Price) : list.OrderBy(x => x.Price),
                    _ =>
                        orderByDesc ? list.OrderByDescending(x => x.ProductId) : list.OrderBy(x => x.ProductId),
                };

                return await ordered
                        .Skip((page - 1) * take)
                        .Take(take)
                        .ToListAsync();
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "商品一覧取得処理に失敗しました。");
                throw;
            }
            finally
            {
                logger.LogInformation($"商品一覧取得サービス処理 : 終了");
            }
        }

        public async Task<Product> PostCreate(Product product, int shopId, string userId)
        {
            try
            {
                logger.LogInformation("商品作成サービス処理 : 開始");

                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("空文字または空白です。", nameof(userId));
                }

                Shop shop = default;
                try
                {
                    shop = (await context.Shop
                        .Include(x => x.Owner)
                        .Include(x => x.Products)
                        .Where(x => x.ShopId == shopId)
                        .ToListAsync())
                        .SingleOrDefault(x => x.IsOwner(userId));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "商品取得中にエラーが発生しました。");
                    throw;
                }

                if (shop == null)
                {
                    throw new ArgumentException("該当のショップが存在しません。", nameof(shopId));
                }

                shop.Products.Add(product);
                await context.SaveChangesAsync();
                return product;
            }
            finally
            {
                logger.LogInformation("商品作成サービス処理 : 終了");
            }
        }

        public async Task<Product> Detail(int id)
        {
            try
            {
                logger.LogInformation("商品詳細取得サービス処理 : 開始");

                Product product = default;
                try
                {
                    product = await context.Product
                        .Include(x => x.ProductImages)
                        .FirstOrDefaultAsync(m => m.ProductId == id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "商品取得処理に失敗しました。");
                }

                if (product == default)
                {
                    throw new ArgumentException(nameof(id));
                }

                return product;
            }
            finally
            {
                logger.LogInformation("商品詳細取得サービス処理 : 終了");
            }
        }

        public async Task<ViewModels.Product.EditViewModel> Edit(int id)
        {
            try
            {
                logger.LogInformation("商品編集画面取得サービス処理 : 開始");

                Product product = default;
                try
                {
                    product = await context.Product
                        .Include(p => p.Genres)
                        .FirstOrDefaultAsync(p => p.ProductId == id);
                }
                catch (Exception ex)
                {
                    throw new Exception("商品取得処理に失敗しました", ex);
                }

                if (product == default)
                {
                    throw new ArgumentException(nameof(id));
                }

                IEnumerable<Genre> genres = default;
                IEnumerable<ProductImage> productImages = default;
                try
                {
                    genres = await context.Genre.OrderBy(x => x.Name).ToListAsync();
                    productImages = await context.ProductImages
                        .Where(x => x.Product.ProductId == product.ProductId)
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception("ジャンルor商品画像の取得に失敗しました", ex);
                }

                var vm = new ViewModels.Product.EditViewModel(
                    product,
                    genres,
                    product.Genres.Select(x => x.GenreId),
                    productImages);

                return vm;
            }
            finally
            {
                logger.LogInformation("商品編集画面取得サービス処理 : 終了");
            }
        }

        public async Task<Product> EditPost(Product product, IEnumerable<int> genreIds, IEnumerable<IFormFile> files)
        {
            try
            {
                logger.LogInformation("商品更新処理 : 開始");

                Product productToUpdate = default;
                try
                {
                    // 現在の商品情報を取得
                    productToUpdate = await context.Product
                        .Include(p => p.Genres)
                        .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
                }
                catch (Exception ex)
                {
                    throw new Exception("商品取得処理に失敗しました。", ex);
                }

                if (productToUpdate == default)
                {
                    throw new ArgumentException(nameof(product));
                }

                // 商品情報を更新
                productToUpdate.Description = product.Description;
                productToUpdate.Discount = product.Discount;
                productToUpdate.Name = product.Name;
                productToUpdate.Price = product.Price;

                // 現在設定されているジャンルIDを保存
                var beforeGenreIds = productToUpdate.Genres.Select(g => g.GenreId).ToList();
                // PostされたジャンルIDを保存
                var afterGenreIds = genreIds;
                // PostジャンルIDと現在ジャンルIDの差 == 追加差分を抽出
                var addGenreIds = afterGenreIds.Except(beforeGenreIds).ToList();
                // 現在ジャンルIDとPostジャンルIDの差 == 削除差分を抽出
                var removeGenreIds = beforeGenreIds.Except(afterGenreIds).ToList();

                // 追加差分を設定
                var addGenres = await context.Genre.Where(x => addGenreIds.Contains(x.GenreId)).ToListAsync();
                productToUpdate.Genres.AddRange(addGenres);

                // 削除差分を設定
                var removeGenres = removeGenreIds
                    .Select(id => productToUpdate.Genres.Single(g => g.GenreId == id))
                    .ToList();
                removeGenres.ForEach(genre => productToUpdate.Genres.Remove(genre));

                // 商品画像ファイルをサーバー内に保存
                IEnumerable<string> savePaths = default;
                try
                {
                    savePaths = await Task.WhenAll(
                        files.Select(file =>
                            PIS.SaveUploadFile(file, PIS.GetGuidFileName(Path.GetExtension(file.FileName)))));
                }
                catch (Exception ex)
                {
                    throw new Exception("商品画像ファイルの保存処理に失敗しました。", ex);
                }

                try
                {
                    // 商品画像ファイルの保存場所をDBに登録
                    var productImages = savePaths.Select(savePath => new ProductImage
                    {
                        Uri = savePath,
                        Timestamp = DateTime.Now,
                        Product = productToUpdate,
                    });
                    await context.ProductImages.AddRangeAsync(productImages);

                    // 商品情報を更新
                    await context.SaveChangesAsync();

                    return productToUpdate;
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
            finally
            {
                logger.LogInformation("商品更新処理 : 終了");
            }
        }

        public async Task<Product> Delete(int id)
        {
            try
            {
                logger.LogInformation("商品削除画面取得処理 : 開始");

                var product = await context.Product
                    .FirstOrDefaultAsync(m => m.ProductId == id);
                if (product == default)
                {
                    throw new ArgumentException(nameof(id));
                }

                return product;
            }
            finally
            {
                logger.LogInformation("商品削除画面取得処理 : 終了");
            }
        }

        public async Task DeleteConfirmed(int id)
        {
            try
            {
                logger.LogInformation("商品削除処理 : 開始");

                Product product = default;
                try
                {
                    product = await context.Product.SingleOrDefaultAsync(x => x.ProductId == id);
                }
                catch (Exception ex)
                {
                    throw new Exception("商品取得処理に失敗しました", ex);
                }
                
                try
                {
                    context.Product.Remove(product);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception("商品削除処理に失敗しました", ex);
                }
            }
            finally
            {
                logger.LogInformation("商品削除処理 : 終了");
            }
        }

        public IEnumerable<IValidateProductImageFileResult> ValidateProductImageFiles(IEnumerable<IFormFile> files)
        {
            try
            {
                logger.LogInformation("商品イメージ検証処理 : 開始");
                return files
                    .Select(x => ValidateProductImageFileResult.Validate(x, PIS, logger))
                    .Where(x => !x.IsOk)
                    .ToList();
            }
            finally
            {
                logger.LogInformation("商品イメージ検証処理 : 開始");
            }
        }

        public async Task<IEnumerable<Genre>> GetSortedAllGenres()
        {
            try
            {
                logger.LogInformation("ジャンル取得処理 : 開始");
                return await context.Genre.OrderBy(x => x.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("ジャンル取得処理に失敗しました。", ex);
            }
            finally
            {
                logger.LogInformation("ジャンル取得処理 : 終了");
            }
        }

        public class ValidateProductImageFileResult : IValidateProductImageFileResult
        {
            public string FileName { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public bool IsOk => FileName == string.Empty && Message == string.Empty;

            public static readonly ValidateProductImageFileResult Ok = new ValidateProductImageFileResult();

            public static ValidateProductImageFileResult Validate(
                IFormFile file, IProductImageService pis, ILogger logger)
            {
                if (!(file.Length > 0))
                {
                    logger.LogInformation("0 bytes file: {0}", file.FileName);
                    return new ValidateProductImageFileResult
                    {
                        FileName = file.FileName,
                        Message = "0 bytes file",
                    };
                }

                if (!pis.IsJpeg(file) && !pis.IsPng(file))
                {
                    logger.LogWarning("Not jpeg or png: {0}", file.FileName);
                    return new ValidateProductImageFileResult
                    {
                        FileName = file.FileName,
                        Message = "not jpeg or png."
                    };
                }

                return ValidateProductImageFileResult.Ok;
            }
        }
    }
}
