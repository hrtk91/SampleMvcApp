using SampleMvcApp.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SampleMvcApp.ViewModels.Product
{
    public class EditViewModel
    {
        public Models.Product Product { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public IEnumerable<int> SelectedGenres { get; set; }
        public IEnumerable<IFormFile> Files { get; set; }

        public IEnumerable<ProductImage> ProductImages { get; set; }

        public EditViewModel(Models.Product product, IEnumerable<Genre> genres, IEnumerable<int> selectedGenres)
        {
            Product = product;
            Genres = genres;
            SelectedGenres = selectedGenres;
        }

        public EditViewModel(Models.Product product, IEnumerable<Genre> genres, IEnumerable<int> selectedGenres, IEnumerable<IFormFile> files)
            : this(product, genres, selectedGenres)
        {
            Files = files;
        }

        public EditViewModel(Models.Product product, IEnumerable<Genre> genres, IEnumerable<int> selectedGenres, IEnumerable<ProductImage> productImages)
            : this(product, genres, selectedGenres, Enumerable.Empty<IFormFile>())
        {
            ProductImages = productImages;
        }
    }
}