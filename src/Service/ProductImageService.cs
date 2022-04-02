using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SampleMvcApp.Services.Interface;

namespace SampleMvcApp.Services
{
    public class ProductImageService : IProductImageService
    {
        private IFileWriter _fileWriter;
        private ILogger<ProductImageService> _logger;
        private IWebHostEnvironment _environment;
        private IConfiguration _configuration;

        public ProductImageService(IFileWriter fileWriter, ILogger<ProductImageService> logger, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _fileWriter = fileWriter;
            _logger = logger;
            _environment = environment;
            _configuration = configuration;
        }

        public bool IsJpeg(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            using (var reader = new BinaryReader(stream))
            {
                var soi = reader.ReadBytes(2);
                var eoi = reader.ReadBytes((int)stream.Length).TakeLast(2);
                var bytes = Enumerable.Concat(soi, eoi).ToArray();
                return (bytes.Length >= 4) && IsJpeg(bytes);
            }
        }

        public bool IsPng(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            using (var reader = new BinaryReader(stream))
            {
                var signature = reader.ReadBytes(8);
                return IsPng(signature);
            }
        }

        /// <summary>
        /// 参考URL:https://qiita.com/kazuaki0213/items/d3e71fe203b4f1d19abc
        /// </summary>
        public bool IsJpeg(byte[] bytes) =>
            (bytes[0] == 0xFF && bytes[1] == 0xd8) &&
            (bytes[2] == 0xff && bytes[3] == 0xd9);

        /// <summary>
        /// 参考URL:https://www.setsuki.com/hsp/ext/png.htm
        /// ヘッダ8byteのみで判定している
        /// </summary>
        public bool IsPng(byte[] bytes) =>
            (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
             bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A);

        public string GetGuidFileName(string extension = "", string format = "N") =>
            $"{(Guid.NewGuid().ToString("N"))}{extension}";

        public string SaveDirectoryName => _configuration.GetValue<string>("PRODUCTIMAGE_DIRECTORYNAME") ?? string.Empty;
        public string RootDirectory => _environment.WebRootPath;

        /// <summary>
        /// ファイルを保存してファイルパスを返却する
        /// <return>RootDirectoryを"~/"として始まる保存先パス</return>
        /// </summary>
        public async Task<string> SaveUploadFile(IFormFile file, string filename)
        {
            var dirname = SaveDirectoryName;
            var dirpath = Path.Combine(RootDirectory, dirname);
            Directory.CreateDirectory(dirpath);

            var filepath = Path.Combine(dirpath, filename);

            using (var stream = _fileWriter.Open(filepath, FileMode.OpenOrCreate))
            {
                await file.CopyToAsync(stream);
                _logger.LogInformation("User upload file: {0}", filepath);
            }

            return $"~/{dirname}/{filename}";
        }
    }
}
