using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
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
        public async System.Threading.Tasks.Task<ActionResult<IEnumerable<ModelTask>>> GetAll()
        {
            var tasks = await _db.Tasks.ToListAsync();
            return Ok(tasks);
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
            if (string.IsNullOrWhiteSpace(model.Description))
                throw new InvalidOperationException("La descripción es obligatoria");

            if (model.DueDate <= DateTime.UtcNow)
                throw new InvalidOperationException("La fecha debe ser futura");

            _db.Tasks.Add(model);
            await _db.SaveChangesAsync();
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
            return NoContent();
        }
    }
}
