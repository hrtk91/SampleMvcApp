using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace SampleMvcApp.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        
        public IdentityUser Owner { get; set; }
        public IList<Models.Product> Products { get; set; }

        public int TotalPrice()
        {
            var prices = Products.Select(x => x.Price);
            return prices.Any() ? prices.Aggregate((pre, cur) => pre + cur) : 0;
        }
    }
}
