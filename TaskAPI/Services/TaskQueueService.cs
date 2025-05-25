using System.Reactive.Subjects;
using TaskAPI.Models;

namespace TaskAPI.Services
{
    public class TaskQueueService
    {
        private readonly Queue<Models.Task> _taskQueue = new();
        private bool _isProcessing = false;
        private readonly Subject<Models.Task> _taskProcessed = new();

        public IObservable<Models.Task> TaskProcessed => _taskProcessed;

        public void EnqueueTask(Models.Task task)
        {
            _taskQueue.Enqueue(task);
            ProcessQueue();
        }

        private async void ProcessQueue()
        {
            if (_isProcessing || _taskQueue.Count == 0)
                return;

            _isProcessing = true;

            var taskToProcess = _taskQueue.Dequeue();

            await System.Threading.Tasks.Task.Delay(2000);

            _taskProcessed.OnNext(taskToProcess);
            _isProcessing = false;

            ProcessQueue();
        }

        public int PendingCount => _taskQueue.Count;
    }
}
