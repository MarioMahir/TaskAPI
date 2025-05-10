using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.Models;
using TaskAPI.Helpers;
using static TaskAPI.Helpers.TaskDelegates;
using ModelTask = TaskAPI.Models.Task<string>;

namespace TaskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TasksController(AppDbContext db) => _db = db;

        [HttpGet]
        public async System.Threading.Tasks.Task<ActionResult<IEnumerable<Models.Task<string>>>> GetPendientes()
        {
            var tareasPendientes = await _db.Tasks
                .Where(t => new Func<bool>(() => !t.IsCompleted)())
                .ToListAsync();

            return tareasPendientes;
        }


        [HttpGet("{id}")]
        public async System.Threading.Tasks.Task<ActionResult<ModelTask>> Get(int id)
        {
            var t = await _db.Tasks.FindAsync(id);
            if (t == null) return NotFound(new { error = "Tarea no encontrada" });
            return Ok(t);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult<ModelTask>> Create(ModelTask model)
        {
            ValidarTarea<string> validar = t =>
                !string.IsNullOrWhiteSpace(t.Description) && t.DueDate > DateTime.UtcNow;

            if (!validar(model))
                return BadRequest(new { error = "Descripción vacía o fecha no válida." });

            NotificarCreacion(model);

            _db.Tasks.Add(model);
            await _db.SaveChangesAsync();

            var dias = DiasRestantes(model.DueDate);
            Console.WriteLine($"La tarea vence en {dias} días.");

            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        [HttpPut("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> Update(int id, ModelTask model)
        {
            if (id != model.Id)
                return BadRequest(new { error = "Id de ruta o body distinto" });

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> Delete(int id)
        {
            var t = await _db.Tasks.FindAsync(id);
            if (t == null) return NotFound(new { error = "Tarea no encontrada" });

            _db.Tasks.Remove(t);
            await _db.SaveChangesAsync();
            Console.WriteLine($"[NOTIFICACIÓN] La tarea con ID {id} ha sido eliminada exitosamente.");

            return NoContent();
        }
        
    }
}
