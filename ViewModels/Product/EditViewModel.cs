using SampleMvcApp.Models;
using System.Linq;
using System.Collections.Generic;

namespace SampleMvcApp.ViewModels.Product
{
    public class EditViewModel
    {
        public Models.Product Product { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
        public IEnumerable<int> SelectedGenres { get; set; }

        public EditViewModel(Models.Product product, IEnumerable<Genre> genres, IEnumerable<int> selectedGenres)
        {
            Product = product;
            Genres = genres;
            SelectedGenres = selectedGenres;
        }
    }
}