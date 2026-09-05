using System.ComponentModel.DataAnnotations;

namespace TaskAPI.Models
{
    public class LoginModel
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}
