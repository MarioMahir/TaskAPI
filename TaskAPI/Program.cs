using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.Middleware;
using TaskAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("TaskConnection")));
builder.Services.AddControllers();
builder.Services.AddSingleton<TaskQueueService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var taskQueue = app.Services.GetRequiredService<TaskQueueService>();
taskQueue.TaskProcessed.Subscribe(task =>
{
    Console.WriteLine($"Procesada la tarea con ID {{task.Id}} y descripción: {{task.Description}}");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
