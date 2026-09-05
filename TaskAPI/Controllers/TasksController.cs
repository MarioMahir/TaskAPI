using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.Helpers;
using TaskAPI.Hubs;
using TaskAPI.Services;
using static TaskAPI.Helpers.TaskDelegates;
using ModelTask = TaskAPI.Models.Task;
using TaskFactory = TaskAPI.Factory.TaskFactory;

namespace TaskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly Func<DateTime, int> _diasRestantesMemo;
        private readonly TaskQueueService _taskQueue;
        private readonly IHubContext<TaskHub> _hub;

        // Misma regla para crear, encolar y actualizar: descripción obligatoria y fecha futura.
        private static readonly ValidarTarea<string> _validar = t =>
            !string.IsNullOrWhiteSpace(t.Description) && t.DueDate > DateTime.UtcNow;

        public TasksController(AppDbContext db, TaskQueueService taskQueue, IHubContext<TaskHub> hub)
        {
            _db = db;
            _taskQueue = taskQueue;
            _diasRestantesMemo = Memoizer.Memoize<DateTime, int>(DiasRestantes);
            _hub = hub;
        }

        /// <summary>Lista las tareas. Con ?pendientes=true devuelve solo las no completadas.</summary>
        [HttpGet]
        public async System.Threading.Tasks.Task<ActionResult<IEnumerable<ModelTask>>> GetAll([FromQuery] bool pendientes = false)
        {
            IQueryable<ModelTask> query = _db.Tasks;

            if (pendientes)
                query = query.Where(t => !t.IsCompleted);

            return await query.OrderBy(t => t.DueDate).ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async System.Threading.Tasks.Task<ActionResult<ModelTask>> Get(int id)
        {
            var t = await _db.Tasks.FindAsync(id);
            if (t == null) return NotFound(new { error = "Tarea no encontrada" });
            return Ok(t);
        }

        /// <summary>Crea una tarea a través del Factory y notifica por SignalR.</summary>
        [HttpPost("factory")]
        public async System.Threading.Tasks.Task<ActionResult<ModelTask>> Create(ModelTask model)
        {
            ModelTask tarea;

            try
            {
                tarea = TaskFactory.CreateNormalTask(model.Description, model.DueDate, _validar);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            tarea.ExtraData = model.ExtraData ?? "";

            NotificarCreacion(tarea);

            _db.Tasks.Add(tarea);
            await _db.SaveChangesAsync();

            if (_hub != null)
                await _hub.Clients.All.SendAsync("TareaCreada", tarea);

            var dias = _diasRestantesMemo(model.DueDate);
            Console.WriteLine($"La tarea vence en {dias} días.");

            return CreatedAtAction(nameof(Get), new { id = tarea.Id }, tarea);
        }

        [HttpPut("{id:int}")]
        public async System.Threading.Tasks.Task<IActionResult> Update(int id, ModelTask model)
        {
            if (id != model.Id)
                return BadRequest(new { error = "Id de ruta o body distinto" });

            if (string.IsNullOrWhiteSpace(model.Description))
                return BadRequest(new { error = "La descripción es obligatoria" });

            var existingTask = await _db.Tasks.FindAsync(id);
            if (existingTask == null)
                return NotFound(new { error = "Tarea no encontrada" });

            // Al editar se permite una fecha pasada solo si la tarea queda marcada como completada.
            if (!model.IsCompleted && model.DueDate <= DateTime.UtcNow)
                return BadRequest(new { error = "La fecha límite de una tarea pendiente debe ser futura" });

            existingTask.Description = model.Description;
            existingTask.DueDate = model.DueDate;
            existingTask.IsCompleted = model.IsCompleted;
            existingTask.ExtraData = model.ExtraData ?? "";

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async System.Threading.Tasks.Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest(new { error = "Id inválido" });

            var t = await _db.Tasks.FindAsync(id);
            if (t == null) return NotFound(new { error = "Tarea no encontrada" });

            _db.Tasks.Remove(t);
            await _db.SaveChangesAsync();
            Console.WriteLine($"[NOTIFICACIÓN] La tarea con ID {id} ha sido eliminada exitosamente.");

            return NoContent();
        }

        /// <summary>Guarda la tarea y la manda a la cola reactiva; al procesarse se emite TareaProcesada.</summary>
        [HttpPost("queue")]
        public async System.Threading.Tasks.Task<IActionResult> AddToQueue(ModelTask model)
        {
            if (!_validar(model))
                return BadRequest(new { error = "La tarea necesita descripción y una fecha límite futura" });

            model.Id = 0;
            model.ExtraData ??= "";

            _db.Tasks.Add(model);
            await _db.SaveChangesAsync();

            _taskQueue.EnqueueTask(model);

            return Accepted(new { message = "Tarea encolada exitosamente", id = model.Id });
        }

        [HttpGet("queue/status")]
        public IActionResult GetQueueStatus()
        {
            int cantidad = _taskQueue.GetPendingCount();
            return Ok(new { pendientes = cantidad });
        }
    }
}
