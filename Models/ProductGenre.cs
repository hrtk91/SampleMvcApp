namespace SampleMvcApp.Models
{
    public class ProductGenre
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int GenreId { get; set; }
        public Genre Genre { get; set; }
    }
}
