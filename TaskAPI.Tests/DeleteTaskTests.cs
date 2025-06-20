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
    public class DeleteTaskTests
    {
        private async Task<AppDbContext> DeleteTask()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new AppDbContext(options);

            var task = new TaskAPI.Models.Task
            {
                Description = "Tarea a eliminar",
                DueDate = DateTime.UtcNow.AddDays(2),
                IsCompleted = false,
                ExtraData = "Extra"
            };

            dbContext.Tasks.Add(task);
            await dbContext.SaveChangesAsync();
            return dbContext;
        }

        [Fact]
        public async Task DeleteTaskSuccessfully()
        {
            var dbContext = await DeleteTask();
            var controller = new TasksController(dbContext, null, null);

            var taskToDelete = await dbContext.Tasks.FirstAsync();

            var result = await controller.Delete(taskToDelete.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Null(await dbContext.Tasks.FindAsync(taskToDelete.Id));
        }


    }
}
