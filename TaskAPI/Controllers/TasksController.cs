﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.Models;
using TaskAPI.Helpers;
using static TaskAPI.Helpers.TaskDelegates;
using ModelTask = TaskAPI.Models.Task;
using TaskAPI.Factory;
using TaskFactory = TaskAPI.Factory.TaskFactory;
using TaskAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TaskAPI.Hubs;

namespace TaskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly Func<DateTime, int> _diasRestantesMemo;
        private readonly TaskQueueService _taskQueue;
        private readonly IHubContext<TaskHub> _hub;

        [NonAction]
        public int DiasRestantes(DateTime dueDate)
        {
            return (dueDate - DateTime.UtcNow).Days;
        }

        public TasksController(AppDbContext db, TaskQueueService taskQueue, IHubContext<TaskHub> hub)
        {
            _db = db;
            _taskQueue = taskQueue;
            _diasRestantesMemo = Memoizer.Memoize<DateTime, int>(DiasRestantes);
            _hub = hub;
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

            NotificarCreacion(tarea);

            _db.Tasks.Add(tarea);
            await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("Tarea Creada", tarea);

            var dias = _diasRestantesMemo(model.DueDate);
            Console.WriteLine($"La tarea vence en {dias} días.");

            return CreatedAtAction(nameof(Get), new { id = tarea.Id }, tarea);
        }

        [HttpPut("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> Update(int id, ModelTask model)
        {
            if (model == null)
                return BadRequest(new { error = "Model no puede ser nulo" });

            if (id != model.Id)
                return BadRequest(new { error = "Id de ruta o body distinto" });

            var existingTask = await _db.Tasks.FindAsync(id);
            if (existingTask == null)
                return NotFound("Tarea no encontrada");

            existingTask.Description = model.Description;
            existingTask.DueDate = model.DueDate;
            existingTask.IsCompleted = model.IsCompleted;
            existingTask.ExtraData = model.ExtraData ?? "";

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
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
            int cantidad = _taskQueue.GetPendingCount();
            return Ok(new { pendientes = cantidad });
        }
        
    }
}
