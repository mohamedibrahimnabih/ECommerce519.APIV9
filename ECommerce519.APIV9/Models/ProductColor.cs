using Microsoft.EntityFrameworkCore;

namespace ECommerce.Models
{
    //[PrimaryKey(nameof(ProductId), nameof(Color))]
    public class ProductColor
    {
        public int ProductId { get; set; }
        public Product Product { get; } = null!;
        public string Color { get; set; } = string.Empty;
    }
}
