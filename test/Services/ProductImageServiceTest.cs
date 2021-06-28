using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SampleMvcApp.Services;
using Xunit;

namespace SampleMvcApp.Tests.Services
{
    public class ProductImageServiceTests
    {
        private ServiceProvider _provider;
        private ILogger<ProductImageService> _logger;
        private Mock<IWebHostEnvironment> _Environment;
        private Mock<IProductImageService> _PIS;
        private Mock<IConfiguration> _Configuration;

        public ProductImageServiceTests()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            _provider = services.BuildServiceProvider();
            _logger = _provider.GetService<ILoggerFactory>().CreateLogger<ProductImageService>();


            _Environment = new Mock<IWebHostEnvironment>();
            _PIS = new Mock<IProductImageService>();
            _Configuration = new Mock<IConfiguration>();
        }

        [Fact]
        public void GetGuidFileName_エクステンションあり()
        {
            var psi = new ProductImageService(new FileWriter(), _logger, _Environment.Object, _Configuration.Object);
            var value = psi.GetGuidFileName(".png");
            Assert.Equal(".png", new String(value.TakeLast(4).ToArray()));
        }
    }
}
