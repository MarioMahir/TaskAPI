using System;
using TaskAPI.Helpers;
using TaskAPI.Interfaces;
using TaskAPI.Models;
using static TaskAPI.Helpers.TaskDelegates;

namespace TaskAPI.Factory
{
    public static class TaskFactory
    {
        public static Models.Task CreateNormalTask(string description, DateTime dueDate, ValidarTarea<string> validar)
        {
            var tarea = new Models.Task
            {
                Description = description,
                DueDate = dueDate,
                IsCompleted = false
            };

            if (!validar(tarea))
                throw new InvalidOperationException("La tarea no es válida");

            return tarea;
        }

        public static Models.Task CreateHighPriorityTask(string description, DateTime dueDate, ValidarTarea<string> validar)
        {
            var tarea = CreateNormalTask(description, dueDate, validar);
            tarea.ExtraData = "Alta prioridad";
            return tarea;
        }
    }
}
