using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskAPI.Controllers;
using TaskAPI.Data;
using TaskAPI.Models;
using Xunit;

namespace TaskAPI.Tests
{
    public class AuthControllerTests
    {
        private const string ClaveDePrueba = "clave-de-prueba-suficientemente-larga-para-hmac-sha256";

        private static AppDbContext CrearContexto() =>
            new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static IConfiguration CrearConfiguracion() =>
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JwtSettings:Key", ClaveDePrueba },
                { "JwtSettings:Issuer", "taskapi" },
                { "JwtSettings:Audience", "taskapi" }
            }).Build();

        private static User UsuarioConHash(string correo, string password)
        {
            var user = new User { Correo = correo };
            user.Password = new PasswordHasher<User>().HashPassword(user, password);
            return user;
        }

        [Fact]
        public void Registrarse()
        {
            using var context = CrearContexto();
            var controller = new AuthController(CrearConfiguracion(), context);

            var result = controller.Register(new LoginModel { Correo = "test@dominio.com", Password = "123456" });

            Assert.IsType<OkObjectResult>(result);
            Assert.Single(context.Users);
        }

        [Fact]
        public void RegistrarseGuardaLaPasswordHasheada()
        {
            using var context = CrearContexto();
            var controller = new AuthController(CrearConfiguracion(), context);

            controller.Register(new LoginModel { Correo = "hash@dominio.com", Password = "123456" });

            var user = context.Users.Single();
            Assert.NotEqual("123456", user.Password);
            Assert.Equal(PasswordVerificationResult.Success,
                new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, "123456"));
        }

        [Fact]
        public void RegistrarseCuandoEmailExiste()
        {
            using var context = CrearContexto();
            context.Users.Add(UsuarioConHash("existente@prueba.com", "123456"));
            context.SaveChanges();
            var controller = new AuthController(CrearConfiguracion(), context);

            var result = controller.Register(new LoginModel { Correo = "Existente@prueba.com", Password = "123456" });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void RegistrarseConPasswordCortaFalla()
        {
            using var context = CrearContexto();
            var controller = new AuthController(CrearConfiguracion(), context);

            var result = controller.Register(new LoginModel { Correo = "corta@prueba.com", Password = "123" });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(context.Users);
        }

        [Fact]
        public void LogueoConCredencialesValidas()
        {
            using var context = CrearContexto();
            context.Users.Add(UsuarioConHash("user@dominio.com", "password"));
            context.SaveChanges();
            var controller = new AuthController(CrearConfiguracion(), context);

            var result = controller.Login(new LoginModel { Correo = "user@dominio.com", Password = "password" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("token", okResult.Value!.ToString());
        }

        [Fact]
        public void LoginConCredencialesInvalidas()
        {
            using var context = CrearContexto();
            context.Users.Add(UsuarioConHash("user@dominio.com", "password"));
            context.SaveChanges();
            var controller = new AuthController(CrearConfiguracion(), context);

            var result = controller.Login(new LoginModel { Correo = "user@dominio.com", Password = "otra" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
