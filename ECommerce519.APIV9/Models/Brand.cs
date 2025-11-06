using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
        public string Img { get; set; } = "defaultImg.png";
        //public List<Product> Products { get; set; }
    }
}
