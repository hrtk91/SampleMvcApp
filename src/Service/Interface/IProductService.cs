using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SampleMvcApp.Models;

namespace SampleMvcApp.Services.Interface
{
    public interface IProductService
    {
        public Task<IEnumerable<Product>> Index();

        public Task<Product> Detail(int id);

        public Task<Product> PostCreate(Product product, int shopId, string userId);

        public Task<ViewModels.Product.EditViewModel> Edit(int id);

        public Task<Product> EditPost(Product product, IEnumerable<int> genreIds, IEnumerable<IFormFile> files);

        public Task<Product> Delete(int id);

        public Task DeleteConfirmed(int id);

        public IEnumerable<IValidateProductImageFileResult> ValidateProductImageFiles(IEnumerable<IFormFile> files);

        public Task<IEnumerable<Genre>> GetSortedAllGenres();
    }

    public interface IValidateProductImageFileResult
    {
        public string FileName { get; set; }
        public string Message { get; set; }
        public bool IsOk { get; }
    }
}
