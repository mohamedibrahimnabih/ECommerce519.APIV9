namespace ECommerce519.APIV9.DTOs.Request
{
    public record FilterProductRequest (
        string name, decimal? minPrice, decimal? maxPrice, int? categoryId, int? brandId, bool lessQuantity, bool isHot
    );
}
