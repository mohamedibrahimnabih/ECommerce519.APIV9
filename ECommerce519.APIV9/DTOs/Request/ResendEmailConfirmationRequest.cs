using System.ComponentModel.DataAnnotations;

namespace ECommerce519.APIV9.DTOs.Request
{
    public class ResendEmailConfirmationRequest
    {
        [Required]
        public string UserNameOREmail { get; set; } = string.Empty;
    }
}
