using System.ComponentModel.DataAnnotations;

namespace ECommerce519.APIV9.DTOs.Request
{
    public class ValidateOTPRequest
    {

        [Required]
        public string OTP { get; set; } = string.Empty;

        public string ApplicationUserId { get; set; } = string.Empty;
    }
}
