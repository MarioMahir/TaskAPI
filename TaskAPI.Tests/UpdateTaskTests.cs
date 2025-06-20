using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAPI.Controllers;
using TaskAPI.Data;

namespace TaskAPI.Tests
{
    public class UpdateTaskTests
    {
        private async Task<AppDbContext> UpdateTask()
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                var dbContext = new AppDbContext(options);

                var task = new TaskAPI.Models.Task
                {
                    Description = "Tarea original",
                    DueDate = DateTime.UtcNow.AddDays(3),
                    IsCompleted = false,
                    ExtraData = "Test Extra"
                };

                dbContext.Tasks.Add(task);
                await dbContext.SaveChangesAsync();
                return dbContext;
            }

            [Fact]
            public async Task UpdateTaskSuccessfully()
            {

                var dbContext = await UpdateTask();
                var controller = new TasksController(dbContext, null, null);

                var existingTask = await dbContext.Tasks.FirstAsync();
                var updatedTask = new TaskAPI.Models.Task
                {
                    Id = existingTask.Id,
                    Description = "Tarea actualizada",
                    DueDate = DateTime.UtcNow.AddDays(5),
                    IsCompleted = true,
                    ExtraData = "Actualizado"
                };

                var result = await controller.Update(existingTask.Id, updatedTask);

                Assert.IsType<NoContentResult>(result);

                var taskInDb = await dbContext.Tasks.FindAsync(existingTask.Id);
                Assert.Equal("Tarea actualizada", taskInDb.Description);
                Assert.True(taskInDb.IsCompleted);
                Assert.Equal("Actualizado", taskInDb.ExtraData);
            }
        }
    }
