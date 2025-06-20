using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAPI.Services;

namespace TaskAPI.Tests
{
    public class EnqueueTaskTests
    {
        [Fact]
        public async System.Threading.Tasks.Task EnqueueTask()
        {
            var service = new TaskQueueService();

            var task = new TaskAPI.Models.Task
            {
                Id = 1,
                Description = "Test reactivo",
                DueDate = DateTime.UtcNow.AddDays(1),
                IsCompleted = false
            };

            bool processed = false;

            service.TaskProcessed.Subscribe(t =>
            {
                processed = true;
                Assert.Equal(task.Description, t.Description);
            });

            service.EnqueueTask(task);

            await System.Threading.Tasks.Task.Delay(3000);

            Assert.True(processed);
        }

        [Fact]
        public void QueuePendingCount()
        {
            var queue = new TaskQueueService();
            var task = new TaskAPI.Models.Task
            {
                Description = "Test",
                DueDate = DateTime.UtcNow.AddDays(1),
                IsCompleted = false
            };

            queue.EnqueueTask(task);
            int pending = queue.GetPendingCount();

            Assert.Equal(1, pending);
        }

    }
}
