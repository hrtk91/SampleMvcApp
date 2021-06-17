using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SampleMvcApp.Models
{
    public class Receipt
    {
        [Key]
        public int ReceiptId { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public int Discount { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        
        public Product Product { get; set; }
        public IdentityUser Seller { get; set; }
        public IdentityUser Buyer { get; set; }
    }
}
