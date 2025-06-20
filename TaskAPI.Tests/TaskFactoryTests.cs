using System;
using TaskAPI.Factory;
using TaskAPI.Models;
using TaskAPI.Helpers;
using Xunit;
using static TaskAPI.Helpers.TaskDelegates;

namespace TaskAPI.Tests
{
    public class TaskFactoryTests
    {
        [Fact]
        public void CreateNormalTask()
        {
            ValidarTarea<string> validar = t =>
                !string.IsNullOrWhiteSpace(t.Description) && t.DueDate > DateTime.UtcNow;

            var dueDate = DateTime.UtcNow.AddDays(2);
            var task = TaskAPI.Factory.TaskFactory.CreateNormalTask("Descripción válida", dueDate, validar);

            Assert.NotNull(task);
            Assert.Equal("Descripción válida", task.Description);
            Assert.False(task.IsCompleted);
        }

        [Fact]
        public void CreateNormalTaskInvalid()
        {
            ValidarTarea<string> validar = t =>
                !string.IsNullOrWhiteSpace(t.Description) && t.DueDate > DateTime.UtcNow;

            var dueDate = DateTime.UtcNow.AddDays(-1);

            Assert.Throws<InvalidOperationException>(() =>
                TaskAPI.Factory.TaskFactory.CreateNormalTask("", dueDate, validar));
        }
    }
}
