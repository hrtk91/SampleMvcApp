using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SampleMvcApp.Data;
using SampleMvcApp.Services;
using SampleMvcApp.Services.Interface;

namespace SampleMvcApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddDbContext<SampleMVCAppContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("SampleMVCAppContext")));

            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<SampleMVCAppContext>()
                    // .AddDefaultUI()
                    .AddDefaultTokenProviders();
            
            services.ConfigureApplicationCookie(option =>
            {
                option.AccessDeniedPath = "/StatusCode/AccessDenied";
                option.LoginPath = "/User/Login";
            });

            services.AddRazorPages(options =>
                {
                    options.Conventions.AuthorizePage("/Product");
                })
                .AddRazorRuntimeCompilation();
            
            services
                .AddScoped<IFileWriter, FileWriter>()
                .AddScoped<IProductImageService, ProductImageService>()
                .AddScoped<IProductService, ProductService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            // app.UseHttpsRedirection();
            app.UseStatusCodePagesWithReExecute("/StatusCode", "?code={0}");
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "Role",
                    pattern: "Role/{action=Index}/{name?}",
                    defaults: new { controller = "Role" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Product}/{action=Index}/{id?}");
                // endpoints.MapRazorPages();
            });

            DbInitializer.CreateAdministrator(userManager, roleManager).Wait();
        }
    }
}
