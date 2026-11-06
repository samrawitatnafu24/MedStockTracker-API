namespace MedStockTracker.Application.DTOs.Auth
{
        public class LoginDto
        {
                [System.ComponentModel.DataAnnotations.Required]
                public string Username { get; set; } = string.Empty;

                [System.ComponentModel.DataAnnotations.Required]
                [System.ComponentModel.DataAnnotations.MinLength(8)]
                public string Password { get; set; } = string.Empty;
        }
}
