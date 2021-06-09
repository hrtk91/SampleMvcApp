using SampleMvcApp.Models;
using System.Linq;
using System.Collections.Generic;

namespace SampleMvcApp.ViewModels
{
    public class ProductEditViewModel
    {
        public Product Product { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public IEnumerable<int> SelectedGenres { get; set; }

        public ProductEditViewModel(Product product, IEnumerable<Genre> genres, IEnumerable<int> selectedGenres)
        {
            Product = product;
            Genres = genres;
            SelectedGenres = selectedGenres;
        }
    }
}