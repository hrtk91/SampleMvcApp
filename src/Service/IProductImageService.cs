using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SampleMvcApp.Services
{
    public interface IProductImageService
    {
        string SaveDirectoryName { get; }
        string RootDirectory { get; }

        bool IsJpeg(IFormFile file);

        bool IsPng(IFormFile file);

        /// <summary>
        /// 参考URL:https://qiita.com/kazuaki0213/items/d3e71fe203b4f1d19abc
        /// </summary>
        bool IsJpeg(byte[] bytes);

        /// <summary>
        /// 参考URL:https://www.setsuki.com/hsp/ext/png.htm
        /// ヘッダ8byteのみで判定している
        /// </summary>
        bool IsPng(byte[] bytes);

        string GetGuidFileName(string extension = "", string format = "N");

        Task<string> SaveUploadFile(IFormFile file, string filename);
    }
}
