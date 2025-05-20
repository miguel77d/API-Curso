API Cursos Online 🎓

API RESTful desarrollada con ASP.NET Web API para la gestión de cursos online, implementando autenticación con JWT, control de acceso por roles (profesor, estudiante) y documentación con Swagger.

Autenticación

- Registro y login de usuarios (estudiante y profesor)
- Generación y validación de tokens JWT
- Protección de endpoints con `[Authorize]`

Funcionalidades

Curso
- `POST /api/Curso`: Crear nuevo curso
- `PUT /api/Curso/{id}`: Editar curso
- `DELETE /api/Curso/{id}`: Eliminar curso
- `POST /api/Curso/{id}/inscribir`: Inscripción a curso

Estudiante
- `POST /api/Estudiante/registrarse`
- `POST /api/Estudiante/login`

Profesor
- `POST /api/Profesor/registrarse`
- `POST /api/Profesor/login`

Tecnologías

- ASP.NET Web API
- C#
- Entity Framework
- SQL Server
- Swagger (OpenAPI 3.0)
- JWT Authentication
- Git + GitHub

Documentación interactiva

`https://localhost:5000/swagger` al ejecutar el proyecto localmente.

---

> Proyecto desarrollado como trabajo académico para la materia "Programación II" en la Universidad Adventista del Plata.

