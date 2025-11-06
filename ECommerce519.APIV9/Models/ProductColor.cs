using Microsoft.EntityFrameworkCore;

namespace ECommerce519.APIV9.Models
{
    //[PrimaryKey(nameof(ProductId), nameof(Color))]
    public class ProductColor
    {
        public int ProductId { get; set; }
        public Product Product { get; } = null!;
        public string Color { get; set; } = string.Empty;
    }
}
