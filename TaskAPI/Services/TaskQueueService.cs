using System.Reactive.Linq;
using System.Reactive.Subjects;
using TaskAPI.Models;

namespace TaskAPI.Services
{
    public class TaskQueueService
    {
        private readonly Subject<Models.Task> _taskSubject = new();
        private readonly BehaviorSubject<int> _pendingCount = new(0);
        private readonly Subject<Models.Task> _taskProcessed = new();

        public IObservable<Models.Task> TaskProcessed => _taskProcessed.AsObservable();

        public TaskQueueService()
        {
            _taskSubject
                .Do(_ => _pendingCount.OnNext(_pendingCount.Value + 1)) // Aumenta contador al recibir nueva tarea
                .SelectMany(async task =>
                {
                    await System.Threading.Tasks.Task.Delay(2000); // Simula procesamiento
                    return task;
                })
                .Do(_ => _pendingCount.OnNext(_pendingCount.Value - 1)) // Disminuye contador
                .Subscribe(task =>
                {
                    _taskProcessed.OnNext(task);
                });
        }

        public void EnqueueTask(TaskAPI.Models.Task task)
        {
            _taskSubject.OnNext(task);
        }

        public int GetPendingCount() => _pendingCount.Value;
    }
}
