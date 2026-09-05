using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskAPI.Data;
using TaskAPI.Models;

namespace TaskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        private readonly PasswordHasher<User> _hasher = new();

        public AuthController(IConfiguration config, AppDbContext db)
        {
            _config = config;
            _db = db;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] LoginModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Correo) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { error = "Correo y contraseña son obligatorios." });

            if (model.Password.Length < 6)
                return BadRequest(new { error = "La contraseña debe tener al menos 6 caracteres." });

            var correo = model.Correo.Trim().ToLowerInvariant();

            if (_db.Users.Any(u => u.Correo == correo))
                return BadRequest(new { error = "Correo ya registrado." });

            var user = new User { Correo = correo };
            user.Password = _hasher.HashPassword(user, model.Password);

            _db.Users.Add(user);
            _db.SaveChanges();

            return Ok(new { message = "Usuario registrado." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            var correo = (login.Correo ?? "").Trim().ToLowerInvariant();
            var user = _db.Users.FirstOrDefault(u => u.Correo == correo);

            if (user == null || !VerificarPassword(user, login.Password ?? ""))
                return Unauthorized(new { error = "Credenciales inválidas" });

            var token = GenerateToken(user);
            return Ok(new { token });
        }

        private bool VerificarPassword(User user, string password)
        {
            var resultado = _hasher.VerifyHashedPassword(user, user.Password, password);
            return resultado != PasswordVerificationResult.Failed;
        }

        private string GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings["Key"]
                ?? throw new InvalidOperationException("JwtSettings:Key no está configurada.");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Correo),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
