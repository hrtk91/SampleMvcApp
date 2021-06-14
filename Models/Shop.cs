using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SampleMvcApp.Models
{
    public class Shop
    {
        [Key]
        public int ShopId { get; set; }
        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public IdentityUser Owner { get; set; }
        public IList<Product> Products { get; set; }
        public IList<Receipt> SalesHistories { get; set; }
    }
}
