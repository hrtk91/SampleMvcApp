using Xunit;
using SampleMvcApp.Controllers;
using SampleMvcApp.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Hosting;
using SampleMvcApp.Services.Interface;

namespace SampleMvcApp.Tests
{
    public class ProductControllerTests : IDisposable
    {
        private readonly ITestOutputHelper _stdout;
        private Mock<IWebHostEnvironment> _env;

        public Mock<IProductService> PS { get; }

        private ServiceProvider _provider;
        private ILogger<ProductController> logger;
        private SampleMVCAppContext _context;

        public ProductControllerTests(ITestOutputHelper testOutputHelper)
        {
            _stdout = testOutputHelper;

            _env = new Mock<IWebHostEnvironment>();
            PS = new Mock<IProductService>();
            // _PIS = new Mock<IProductImageService>();
            _provider = CreateServiceProvider();
            logger = _provider.GetService<ILoggerFactory>().CreateLogger<ProductController>();
            _context = _provider.GetService<SampleMVCAppContext>();

            using (var userManager = _provider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = _provider.GetService<RoleManager<IdentityRole>>())
            {
                CreateAdministrator(userManager, roleManager).Wait();
            }
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _provider.Dispose();
        }
        
        [Fact]
        public async Task IndexTest()
        {
            using (var userManager = _provider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = _provider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                var moqProducts = new List<Models.Product>
                {
                    new Models.Product
                    {
                        Name = "Test", Price = 0,
                        Shop = new Models.Shop()
                        {
                            Name = "Shop",
                            Owner = await _context.Users.FirstOrDefaultAsync(x => x.UserName == "admin@mail.com"),
                        }
                    },
                };
                PS.Setup(x =>
                    x.Index(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()))
                    .ReturnsAsync(moqProducts);

                var c = new ProductController(logger, PS.Object);
                #endregion

                #region Action
                var result = await c.Index();
                #endregion

                #region Assert
                var viewresult = Assert.IsType<ViewResult>(result);
                var resultProducts = Assert.IsType<List<Models.Product>>(viewresult.Model);

                Assert.Equal(moqProducts, resultProducts);
                #endregion
            }
        }

        [Fact]
        public async Task CreatePostTest_正常()
        {
            using (var userManager = _provider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = _provider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                var shop = new Models.Shop()
                {
                    ShopId = 1, Name = "Shop",
                    Owner = await _context.Users.FirstOrDefaultAsync(x => x.UserName == DbInitializer.UserName),
                };
                await _context.Shop.AddAsync(shop);
                await _context.SaveChangesAsync();

                var product = new Models.Product()
                {
                    Name = "Test", Price = 0,
                };

                var c = new ProductController(logger, PS.Object);
                c.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new List<Claim>()
                                {
                                    new Claim(ClaimTypes.NameIdentifier, (await _context.Users.FirstAsync(x => x.UserName == DbInitializer.UserName)).Id)
                                },
                                "TestAuthentication"
                            )
                        )
                    }
                };

                PS.Setup(x => x.PostCreate(It.IsAny<Models.Product>(), It.IsAny<int>(), It.IsAny<string>()))
                    .ReturnsAsync(product);
                #endregion

                #region Action
                var result = await c.Create(product, shop.ShopId);
                #endregion
                
                #region Assert
                var viewresult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(ProductController.Index), viewresult.ActionName);
                #endregion
            }
        }

        [Fact]
        public async Task CreatePostTest_ArgumentException()
        {
            using (var userManager = _provider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = _provider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                PS.Setup(x => x.PostCreate(It.IsAny<Models.Product>(), It.IsAny<int>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentException("Test Error", nameof(Models.Product.ProductId)));

                var shop = new Models.Shop
                {
                    ShopId = 1, Name = "Shop",
                    Owner = await _context.Users.FirstOrDefaultAsync(x => x.UserName == DbInitializer.UserName),
                };

                var product = new Models.Product
                {
                    Name = "Test", Price = 0,
                };

                var c = new ProductController(logger, PS.Object);
                c.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new List<Claim>()
                                {
                                    new Claim(ClaimTypes.NameIdentifier, (await _context.Users.FirstAsync(x => x.UserName == DbInitializer.UserName)).Id)
                                },
                                "TestAuthentication"
                            )
                        )
                    }
                };
                #endregion

                #region Action
                var result = await c.Create(product, shop.ShopId);
                #endregion
                
                #region Assert
                var viewresult = Assert.IsType<ViewResult>(result);
                Assert.Equal(product, viewresult.Model);
                // ProdutIdのエラーがModelStateに登録されていることを確認
                viewresult.ViewData.ModelState.TryGetValue(nameof(Models.Product.ProductId), out var value);
                var errMsg = Assert.IsType<string>(value.Errors.Single().ErrorMessage);
                Assert.Equal("Test Error (Parameter 'ProductId')", errMsg);
                #endregion
            }
        }

        [Fact(Skip = "サービスに移動予定")]
        public async Task CreatePostTest_User不一致NotFound()
        {
            using (var userManager = _provider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = _provider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                var shop = new Models.Shop()
                {
                    ShopId = 1, Name = "Shop",
                    Owner = await _context.Users.FirstOrDefaultAsync(x => x.UserName == DbInitializer.UserName),
                };
                await _context.Shop.AddAsync(shop);
                await _context.SaveChangesAsync();

                var product = new Models.Product()
                {
                    Name = "Test", Price = 0,
                };

                var c = new ProductController(logger, PS.Object);
                c.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new List<Claim>()
                                {
                                    // ユーザークレームを設定しない
                                    // new Claim(ClaimTypes.NameIdentifier, (await context.Users.FirstAsync(x => x.UserName == DbInitializer.UserName)).Id)
                                },
                                "TestAuthentication"
                            )
                        )
                    }
                };
                #endregion

                #region Action
                var result = await c.Create(product, shop.ShopId);
                #endregion
                
                #region Assert
                var viewresult = Assert.IsType<NotFoundResult>(result);
                #endregion
            }
        }

        [Fact(Skip = "サービスに移動予定")]
        public async Task CreatePostTest_ShopなしNotFound()
        {
            using (var userManager = _provider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = _provider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                // ショップを作成しない
                // var shop = new Models.Shop()
                // {
                //     ShopId = 1, Name = "Shop",
                //     Owner = await context.Users.FirstOrDefaultAsync(x => x.UserName == DbInitializer.UserName),
                // };
                // await context.Shop.AddAsync(shop);
                // await context.SaveChangesAsync();

                var product = new Models.Product()
                {
                    Name = "Test", Price = 0,
                };

                var c = new ProductController(logger, PS.Object);
                c.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new List<Claim>()
                                {
                                    new Claim(ClaimTypes.NameIdentifier, (await _context.Users.FirstAsync(x => x.UserName == DbInitializer.UserName)).Id)
                                },
                                "TestAuthentication"
                            )
                        )
                    }
                };
                #endregion

                #region Action
                // ShopIdを1にしておく
                var result = await c.Create(product, 1);
                #endregion
                
                #region Assert
                var viewresult = Assert.IsType<NotFoundResult>(result);
                #endregion
            }
        }

        [Fact(Skip = "サービスに移動予定")]
        public async Task CreatePostTest_Shopのオーナー不一致NotFound()
        {
            using (var userManager = _provider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = _provider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                var shop = new Models.Shop()
                {
                    ShopId = 1, Name = "Shop",
                    Owner = new IdentityUser("不一致"),
                };
                await _context.Shop.AddAsync(shop);
                await _context.SaveChangesAsync();

                var product = new Models.Product()
                {
                    Name = "Test", Price = 0,
                };

                var c = new ProductController(logger, PS.Object);
                c.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new List<Claim>()
                                {
                                    new Claim(ClaimTypes.NameIdentifier, (await _context.Users.FirstAsync(x => x.UserName == DbInitializer.UserName)).Id)
                                },
                                "TestAuthentication"
                            )
                        )
                    }
                };
                #endregion

                #region Action
                var result = await c.Create(product, shop.ShopId);
                #endregion
                
                #region Assert
                var viewresult = Assert.IsType<NotFoundResult>(result);
                #endregion
            }
        }

        [Fact(Skip = "サービスに移動予定")]
        public async Task CreatePostTest_モデル状態不正NotFound()
        {
            using (var userManager = _provider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = _provider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                var shop = new Models.Shop()
                {
                    ShopId = 1, Name = "Shop",
                    Owner = await _context.Users.FirstOrDefaultAsync(x => x.UserName == DbInitializer.UserName),
                };
                await _context.Shop.AddAsync(shop);
                await _context.SaveChangesAsync();

                var product = new Models.Product()
                {
                    Name = "Test", Price = 0,
                };

                var c = new ProductController(logger, PS.Object);
                c.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new List<Claim>()
                                {
                                    new Claim(ClaimTypes.NameIdentifier, (await _context.Users.FirstAsync(x => x.UserName == DbInitializer.UserName)).Id)
                                },
                                "TestAuthentication"
                            )
                        )
                    }
                };

                // モデルエラーを登録
                c.ModelState.AddModelError("test", "test error");
                #endregion

                #region Action
                var result = await c.Create(product, shop.ShopId);
                #endregion
                
                #region Assert
                var viewresult = Assert.IsType<ViewResult>(result);
                Assert.Equal(viewresult.Model, product);
                #endregion
            }
        }

        private ServiceProvider CreateServiceProvider()
        {
            // おそらく参考文献：https://devadjust.exblog.jp/27677443/
            // 　（あとから探したのでどうやってこのコードにたどり着いたのか不明）
            // 参考文献その2:https://qiita.com/okazuki/items/239ca5ef46e5a085e085
            // 　その2はAddTransient,AddSingleton,AddScopedの違いが書いてあるのでありがたい
            var services = new ServiceCollection();
            services.AddDbContext<SampleMVCAppContext>(o => o.UseInMemoryDatabase("InMemoryDb"), ServiceLifetime.Scoped);
            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<SampleMVCAppContext>();
            services.AddLogging();

            var provider = services.BuildServiceProvider();
            return provider;
        }

        private async Task CreateAdministrator(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole("Admin") { NormalizedName = "ADMIN" });
            await DbInitializer.CreateAdministrator(userManager, roleManager);
        }
    }
}
