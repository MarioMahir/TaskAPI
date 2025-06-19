using System.ComponentModel.DataAnnotations;

namespace TaskAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Correo { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
