namespace ECommerce519.APIV9.DTOs.Request
{
    public class FilterProductResponse
    {
        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public bool LessQuantity { get; set; }
        public bool IsHot { get; set; }
    }
}
