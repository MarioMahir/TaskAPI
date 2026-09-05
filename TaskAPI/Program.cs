using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskAPI.Data;
using TaskAPI.Hubs;
using TaskAPI.Middleware;
using TaskAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Base de datos: SQL Server por defecto; en memoria si "UseInMemoryDatabase": true
// (útil para probar la API sin instalar SQL Server).
if (builder.Configuration.GetValue<bool>("UseInMemoryDatabase"))
{
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("TaskAPIDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("TaskConnection")));
}

builder.Services.AddControllers();
builder.Services.AddSingleton<TaskQueueService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task API",
        Version = "v1",
        Description = "API de tareas con JWT, SignalR y cola reactiva. Regístrate en /api/Auth/register, haz login y usa el botón Authorize con el token."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Pega solo el token JWT (sin la palabra Bearer).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// JWT: la clave viene de user-secrets o variables de entorno, nunca del repositorio.
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Key"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException(
        "JwtSettings:Key no está configurada o es muy corta (mínimo 32 caracteres). " +
        "Configúrala con: dotnet user-secrets set \"JwtSettings:Key\" \"<clave-larga>\" --project TaskAPI");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[JWT ERROR]: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMe", policy =>
        policy.WithOrigins("http://localhost:9095", "http://localhost:5058", "https://localhost:7153")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Cuando una tarea sale de la cola, se avisa por consola y por SignalR a todos los clientes.
var hubContext = app.Services.GetRequiredService<IHubContext<TaskHub>>();
var taskQueue = app.Services.GetRequiredService<TaskQueueService>();
taskQueue.TaskProcessed.Subscribe(async task =>
{
    Console.WriteLine($"[EVENTO] Procesada la tarea con ID {task.Id} y descripción: {task.Description}");
    await hubContext.Clients.All.SendAsync("TareaProcesada", task);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseRouting();
app.UseCors("AllowMe");

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();
app.MapHub<TaskHub>(TaskHub.HUB_ENDPOINT);

app.Run();
