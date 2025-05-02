# TaskAPI

**TaskAPI** es una API web RESTful desarrollada en **ASP.NET Core** para la gestión de tareas (Tasks). Su objetivo es permitir operaciones básicas de tipo CRUD (Crear, Leer, Actualizar y Eliminar) sobre un conjunto de tareas.

---

## 📌 Características

- CRUD completo para la gestión de tareas.
- Validaciones para permitir la entrada de información.
- Middleware.
- Arquitectura limpia con separación por capas.
- Retorno de respuestas adecuadas con códigos de estado HTTP.

---

## 🧪 Pruebas y uso de la API

### Endpoints disponibles

| Método | Ruta                 | Descripción                      |
|--------|----------------------|----------------------------------|
| GET    | `/api/Tasks`         | Obtener todas las tareas.        |
| POST   | `/api/Tasks`         | Crear una nueva tarea.           |
| GET    | `/api/Tasks/{id}`    | Obtener una tarea por su ID.     |
| PUT    | `/api/Tasks/{id}`    | Actualizar una tarea existente.  |
| DELETE | `/api/Tasks/{id}`    | Eliminar una tarea.              |

### Ejemplo de JSON para crear una tarea:

```json
{
  "id": 0,
  "description": "Acabar mi web API",
  "dueDate": "2025-06-01T00:00:00Z"
  "isCompleted": true,
  "extraData": "Prioridad alta"
}
```

---

## 📂 Estructura del proyecto

```
TaskAPI/
│
├── Controllers/
│   └── TasksController.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── Models/
│   └── Task.cs
│
├── Middlewares/
│   └── ErrorHandlerMiddleware.cs
│
├── Program.cs
├── appsettings.json
└── README.md
```

---

## 🔐 Seguridad

- Middleware global para capturar excepciones y retornar mensajes claros.
- Uso de HTTP status codes adecuados (`400`, `404`, `201`, `204`).

---

## ❗Nota

Considero que este proyecto cumple con lo solicitado por el maestro. De no ser así, cualquier sugerencia, detalle o crítica constructiva es aceptado.

---
