# TaskAPI

**TaskAPI** es una API web RESTful desarrollada en **ASP.NET Core** para la gestión de tareas (Tasks). Su objetivo es permitir operaciones básicas de tipo CRUD (Crear, Leer, Actualizar y Eliminar) sobre un conjunto de tareas, simulando un entorno de trabajo funcional mediante el uso de una base de datos en memoria (InMemoryDatabase).

Este proyecto ha sido creado con fines educativos y para demostrar la implementación de buenas prácticas en el desarrollo de APIs utilizando Entity Framework Core y ASP.NET Core.

---

## ?? Características

- CRUD completo de tareas.
- Validaciones básicas de entrada.
- Manejador global de errores (middleware personalizado).
- Conexión a base de datos en memoria (EF Core InMemory).
- Arquitectura limpia con separación por capas (`Controllers`, `Models`, `Data`).
- Retorno de respuestas adecuadas con códigos de estado HTTP.

---

## ?? Requisitos

- [.NET SDK 8.0 o superior](https://dotnet.microsoft.com/download)
- Visual Studio 2022 / VS Code (opcional)

---

## ??? Configuración del proyecto

1. **Clona el repositorio**:
   ```bash
   git clone https://github.com/tu-usuario/TaskAPI.git
   cd TaskAPI
