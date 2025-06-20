using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskAPI.Controllers;
using TaskAPI.Data;
using TaskAPI.Models;
using TaskAPI.Services;
using Xunit;

namespace TaskAPI.Tests
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly AppDbContext _context;

        public AuthControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestAuthDb")
                .Options;
            _context = new AppDbContext(options);

            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Key", "clave-secreta-de-prueba"},
                {"JwtSettings:Issuer", "testissuer"},
                {"JwtSettings:Audience", "testaudience"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _controller = new AuthController(configuration, _context);
        }

        [Fact]
        public void Registrarse()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("RegisterUserDb").Options;

            using var context = new AppDbContext(options);
            var controller = new AuthController(new ConfigurationBuilder().Build(), context);

            var newUser = new User { Correo = "test@dominio.com", Password = "1234" };
            var result = controller.Register(newUser);

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Single(context.Users);
        }

        [Fact]
        public void RegistrarseCuandoEmailExiste()
        {
            _context.Users.Add(new User { Correo = "existente@prueba.com", Password = "1234" });
            _context.SaveChanges();

            var user = new User { Correo = "existente@prueba.com", Password = "1234" };

            var result = _controller.Register(user);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void LogueoConCredencialesValidas()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("LoginValidDb").Options;

            using var context = new AppDbContext(options);
            context.Users.Add(new User { Correo = "user@dominio.com", Password = "pass" });
            context.SaveChanges();

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
    {
        { "JwtSettings:Key", "wfYVAIz0OhjHZQCGWMEtJ/uFWS3za1htyBBiSS9UEOzWVOaP+Dfcf2cJfyXyYJRZfdgT5ZYszQdUgU5kJH2ZdObXiQcrvOhTjyHDZBhvSklwDKECkx36BIK6mR4reD3cw/e3Kh4YLO37exsuDgbYb7G+ikoZ86pV8aKmkKsQWCI" },
        { "JwtSettings:Issuer", "taskapi" },
        { "JwtSettings:Audience", "taskapi" }
    }).Build();

            var controller = new AuthController(configuration, context);
            var login = new LoginModel { Correo = "user@dominio.com", Password = "pass" };

            var result = controller.Login(login);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("token", okResult.Value.ToString());
        }

        [Fact]
        public void LoginConCredencialesInvalidas()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("LoginInvalidDb").Options;

            using var context = new AppDbContext(options);
            context.Users.Add(new User { Correo = "user@dominio.com", Password = "pass" });
            context.SaveChanges();

            var controller = new AuthController(new ConfigurationBuilder().Build(), context);
            var login = new LoginModel { Correo = "wrong@dominio.com", Password = "wrong" };

            var result = controller.Login(login);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
