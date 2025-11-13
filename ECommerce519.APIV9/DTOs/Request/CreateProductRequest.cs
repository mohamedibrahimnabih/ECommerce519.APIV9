namespace ECommerce519.APIV9.DTOs.Request
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }

        public IFormFile Img { get; set; } = default!;
        public List<IFormFile>? SubImgs { get; set; }
        public List<string>? Colors { get; set; }
    }
}
