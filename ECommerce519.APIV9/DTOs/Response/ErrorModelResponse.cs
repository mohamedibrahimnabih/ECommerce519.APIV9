namespace ECommerce519.APIV9.DTOs.Response
{
    public class ErrorModelResponse
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
    }
}
