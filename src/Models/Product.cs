using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SampleMvcApp.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Range(0, 1000000)]
        public int Price { get; set; }

        [Range(0, 100)]
        public int Discount { get; set; }

        public Shop Shop { get; set; }

        public IList<Cart> Carts { get; set; } = new List<Cart>();

        public IList<Genre> Genres { get; set; } = new List<Genre>();

        public IList<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}
