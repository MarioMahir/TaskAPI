using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Controllers;
using TaskAPI.Data;
using TaskAPI.Services;
using ModelTask = TaskAPI.Models.Task;

namespace TaskAPI.Tests
{
    public class TasksControllerTests
    {
        private static AppDbContext CrearContexto() =>
            new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static TasksController CrearController(AppDbContext db) =>
            new TasksController(db, new TaskQueueService(), null!);

        [Fact]
        public async System.Threading.Tasks.Task GetAllDevuelveTodasYPendientesFiltra()
        {
            using var db = CrearContexto();
            db.Tasks.AddRange(
                new ModelTask { Description = "Hecha", DueDate = DateTime.UtcNow.AddDays(1), IsCompleted = true },
                new ModelTask { Description = "Pendiente", DueDate = DateTime.UtcNow.AddDays(2), IsCompleted = false });
            await db.SaveChangesAsync();
            var controller = CrearController(db);

            var todas = (await controller.GetAll()).Value!;
            var pendientes = (await controller.GetAll(pendientes: true)).Value!;

            Assert.Equal(2, todas.Count());
            Assert.Single(pendientes);
            Assert.Equal("Pendiente", pendientes.First().Description);
        }

        [Fact]
        public async System.Threading.Tasks.Task CrearConFechaPasadaFalla()
        {
            using var db = CrearContexto();
            var controller = CrearController(db);

            var result = await controller.Create(new ModelTask
            {
                Description = "Vencida",
                DueDate = DateTime.UtcNow.AddDays(-1)
            });

            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Empty(db.Tasks);
        }

        [Fact]
        public async System.Threading.Tasks.Task EncolarSinDescripcionFalla()
        {
            using var db = CrearContexto();
            var controller = CrearController(db);

            var result = await controller.AddToQueue(new ModelTask
            {
                Description = "   ",
                DueDate = DateTime.UtcNow.AddDays(1)
            });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(db.Tasks);
        }

        [Fact]
        public async System.Threading.Tasks.Task EncolarGuardaYRespondeAccepted()
        {
            using var db = CrearContexto();
            var controller = CrearController(db);

            var result = await controller.AddToQueue(new ModelTask
            {
                Description = "Procesar",
                DueDate = DateTime.UtcNow.AddDays(1)
            });

            Assert.IsType<AcceptedResult>(result);
            Assert.Single(db.Tasks);
        }

        [Fact]
        public async System.Threading.Tasks.Task ActualizarPendienteConFechaPasadaFalla()
        {
            using var db = CrearContexto();
            var tarea = new ModelTask { Description = "Original", DueDate = DateTime.UtcNow.AddDays(3) };
            db.Tasks.Add(tarea);
            await db.SaveChangesAsync();
            var controller = CrearController(db);

            var result = await controller.Update(tarea.Id, new ModelTask
            {
                Id = tarea.Id,
                Description = "Editada",
                DueDate = DateTime.UtcNow.AddDays(-2),
                IsCompleted = false
            });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Original", (await db.Tasks.FindAsync(tarea.Id))!.Description);
        }

        [Fact]
        public async System.Threading.Tasks.Task ActualizarIdDistintoFalla()
        {
            using var db = CrearContexto();
            var controller = CrearController(db);

            var result = await controller.Update(1, new ModelTask { Id = 2, Description = "x", DueDate = DateTime.UtcNow.AddDays(1) });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task EliminarInexistenteDevuelve404()
        {
            using var db = CrearContexto();
            var controller = CrearController(db);

            var result = await controller.Delete(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
