using System.ComponentModel.DataAnnotations;

namespace TaskAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Correo { get; set; } = "";

        /// <summary>Hash de la contraseña (PasswordHasher). Nunca se guarda en texto plano.</summary>
        [Required]
        public string Password { get; set; } = "";
    }
}
