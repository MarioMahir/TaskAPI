using Microsoft.VisualBasic;

namespace TaskAPI.Helpers
{
    public static class TaskDelegates
    {
        public delegate bool ValidarTarea<T>(TaskAPI.Models.Task<T> tarea);

        public static Func<DateTime, int> DiasRestantes = DueDate =>
        {
            return (DueDate - DateTime.UtcNow).Days;
        };

        public static Action<TaskAPI.Models.Task<string>> NotificarCreacion = t => 
        {
            Console.WriteLine($"[NOTIFICACIÓN] Se creó la tarea: {t.Description} - Fecha límite: {t.DueDate}");
        };
    }
}
