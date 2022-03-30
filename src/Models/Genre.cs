using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SampleMvcApp.Models
{
    public class Genre {
        [Key]
        public int GenreId { get; set; }
        
        [Required]
        public string Name { get; set; }

        public IList<Product> Products { get; set; } = new List<Product>();
    }
}
