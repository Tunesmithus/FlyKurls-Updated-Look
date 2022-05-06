using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyKurls.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string SKU { get; set; }

        [Required]
        public string Designer { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 1 - 50")]

        public double Price { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "List Price")]
        public double ListPrice { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 51 - 100")]
        public double Price50 { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price for 100+")]
        public double Price100 { get; set; }

        [ValidateNever]
        public string ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ValidateNever]
        [Display(Name = "Category")]

        public Category Category { get; set; }

        [Display(Name ="Hat Type")]
        public int HatTypeId { get; set; }

        [Required]
        [ValidateNever]
        public HatType HatType { get; set; }
    }
}
