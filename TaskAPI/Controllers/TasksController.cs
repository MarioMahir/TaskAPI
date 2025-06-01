using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.Models;
using TaskAPI.Helpers;
using static TaskAPI.Helpers.TaskDelegates;
using ModelTask = TaskAPI.Models.Task;
using TaskAPI.Factory;
using TaskFactory = TaskAPI.Factory.TaskFactory;
using TaskAPI.Services;

namespace TaskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly Func<DateTime, int> _diasRestantesMemo;
        private readonly TaskQueueService _taskQueue;

        public int DiasRestantes(DateTime dueDate)
        {
            return (dueDate - DateTime.UtcNow).Days;
        }

        public TasksController(AppDbContext db, TaskQueueService taskQueue)
        {
            _db = db;
            _taskQueue = taskQueue;
            _diasRestantesMemo = Memoizer.Memoize<DateTime, int>(DiasRestantes);
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<ActionResult<IEnumerable<Models.Task>>> GetPendientes()
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

        [HttpPost("factory")]
        public async System.Threading.Tasks.Task<ActionResult<ModelTask>> Create(ModelTask model)
        {
            ValidarTarea<string> validar = t =>
                !string.IsNullOrWhiteSpace(t.Description) && t.DueDate > DateTime.UtcNow;

            ModelTask tarea;

            try
            {
                tarea = TaskFactory.CreateNormalTask(model.Description, model.DueDate, validar);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            NotificarCreacion(model);

            _db.Tasks.Add(model);
            await _db.SaveChangesAsync();

            var dias = _diasRestantesMemo(model.DueDate);
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

        [HttpPost("queue")]
        public async System.Threading.Tasks.Task<IActionResult> AddToQueue(ModelTask model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return BadRequest("Descripción inválida");

            _db.Tasks.Add(model);
            await _db.SaveChangesAsync();

            _taskQueue.EnqueueTask(model);

            return Ok(new { message = "Tarea encolada exitosamente" });

        }

        [HttpGet("queue/status")]
        public IActionResult GetQueueStatus()
        {
            int cantidad = _taskQueue.PendingCount;
            return Ok(new { pendientes = cantidad });
        }
        
    }
}
