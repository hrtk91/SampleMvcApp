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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;

namespace SampleMvcApp.Tests
{
    public class ProductControllerTests : IDisposable
    {
        private readonly ITestOutputHelper StdOut;
        public ProductControllerTests(ITestOutputHelper testOutputHelper)
        {
            StdOut = testOutputHelper;

            using (var scope = CreateServiceProvider().CreateScope())
            using (var context = scope.ServiceProvider.GetService<SampleMVCAppContext>())
            using (var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>())
            {
                CreateAdministrator(userManager, roleManager).Wait();
            }
        }

        public void Dispose()
        {
            using (var scope = CreateServiceProvider().CreateScope())
            using (var context = scope.ServiceProvider.GetService<SampleMVCAppContext>())
            {
                context.Database.EnsureDeleted();
            }
        }
        
        [Fact]
        public async Task IndexTest()
        {
            using (var scope = CreateServiceProvider().CreateScope())
            using (var context = scope.ServiceProvider.GetService<SampleMVCAppContext>())
            using (var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                var addProduct = new Models.Product()
                {
                    Name = "Test", Price = 0,
                    Shop = new Models.Shop()
                    {
                        Name = "Shop",
                        Owner = await context.Users.FirstOrDefaultAsync(x => x.UserName == "admin@mail.com"),
                    }
                };
                await context.Product.AddAsync(addProduct);
                await context.SaveChangesAsync();

                var c = new ProductController(context);
                #endregion

                #region Action
                var result = await c.Index();
                #endregion

                #region Assert
                var viewresult = Assert.IsType<ViewResult>(result);
                var products = Assert.IsType<List<Models.Product>>(viewresult.Model);

                var product = products.First();
                Assert.Equal(addProduct.Name, product.Name);
                Assert.Equal(addProduct.Shop.Name, product.Shop.Name);
                Assert.Equal(addProduct.Shop.Owner.UserName, product.Shop.Owner.UserName);
                #endregion
            }
        }

        [Fact]
        public async Task CreatePostTest_正常()
        {
            using (var scope = CreateServiceProvider().CreateScope())
            using (var context = scope.ServiceProvider.GetService<SampleMVCAppContext>())
            using (var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                var shop = new Models.Shop()
                {
                    ShopId = 1, Name = "Shop",
                    Owner = await context.Users.FirstOrDefaultAsync(x => x.UserName == DbInitializer.UserName),
                };
                await context.Shop.AddAsync(shop);
                await context.SaveChangesAsync();

                var product = new Models.Product()
                {
                    Name = "Test", Price = 0,
                };

                var c = new ProductController(context);
                c.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new List<Claim>()
                                {
                                    new Claim(ClaimTypes.NameIdentifier, (await context.Users.FirstAsync(x => x.UserName == DbInitializer.UserName)).Id)
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
                var viewresult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(ProductController.Index), viewresult.ActionName);
                #endregion
            }
        }

        [Fact]
        public async Task CreatePostTest_User不一致NotFound()
        {
            using (var scope = CreateServiceProvider().CreateScope())
            using (var context = scope.ServiceProvider.GetService<SampleMVCAppContext>())
            using (var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                var shop = new Models.Shop()
                {
                    ShopId = 1, Name = "Shop",
                    Owner = await context.Users.FirstOrDefaultAsync(x => x.UserName == DbInitializer.UserName),
                };
                await context.Shop.AddAsync(shop);
                await context.SaveChangesAsync();

                var product = new Models.Product()
                {
                    Name = "Test", Price = 0,
                };

                var c = new ProductController(context);
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

        [Fact]
        public async Task CreatePostTest_ShopなしNotFound()
        {
            using (var scope = CreateServiceProvider().CreateScope())
            using (var context = scope.ServiceProvider.GetService<SampleMVCAppContext>())
            using (var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>())
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

                var c = new ProductController(context);
                c.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new List<Claim>()
                                {
                                    new Claim(ClaimTypes.NameIdentifier, (await context.Users.FirstAsync(x => x.UserName == DbInitializer.UserName)).Id)
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

        [Fact]
        public async Task CreatePostTest_Shopのオーナー不一致NotFound()
        {
            using (var scope = CreateServiceProvider().CreateScope())
            using (var context = scope.ServiceProvider.GetService<SampleMVCAppContext>())
            using (var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                var shop = new Models.Shop()
                {
                    ShopId = 1, Name = "Shop",
                    Owner = new IdentityUser("不一致"),
                };
                await context.Shop.AddAsync(shop);
                await context.SaveChangesAsync();

                var product = new Models.Product()
                {
                    Name = "Test", Price = 0,
                };

                var c = new ProductController(context);
                c.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new List<Claim>()
                                {
                                    new Claim(ClaimTypes.NameIdentifier, (await context.Users.FirstAsync(x => x.UserName == DbInitializer.UserName)).Id)
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

        [Fact]
        public async Task CreatePostTest_モデル状態不正NotFound()
        {
            using (var scope = CreateServiceProvider().CreateScope())
            using (var context = scope.ServiceProvider.GetService<SampleMVCAppContext>())
            using (var userManager = scope.ServiceProvider.GetService<UserManager<IdentityUser>>())
            using (var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>())
            {
                #region Arrange
                var shop = new Models.Shop()
                {
                    ShopId = 1, Name = "Shop",
                    Owner = await context.Users.FirstOrDefaultAsync(x => x.UserName == DbInitializer.UserName),
                };
                await context.Shop.AddAsync(shop);
                await context.SaveChangesAsync();

                var product = new Models.Product()
                {
                    Name = "Test", Price = 0,
                };

                var c = new ProductController(context);
                c.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new List<Claim>()
                                {
                                    new Claim(ClaimTypes.NameIdentifier, (await context.Users.FirstAsync(x => x.UserName == DbInitializer.UserName)).Id)
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
