using System;
using System.ComponentModel.DataAnnotations;

namespace SampleMvcApp.Models
{
    public class ProductImage
    {
        [Key]
        public int ProductImageId { get; set; }

        public string Uri { get; set; }

        public DateTime Timestamp { get; set; }

        public Product Product { get; set; }
    }
}
