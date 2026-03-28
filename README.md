# Gestión de Usuarios y Roles API

API REST desarrollada en ASP.NET Core 8.0 para la gestión de usuarios, roles y autenticación.

**Práctica 2 – Despliegue de Software**  
Contenerización y Orquestación con Docker y Kubernetes

---

## Integrantes del Grupo

| Nombre Completo    |
|---|---|
- Juana Ospina
- David Viloria
- Santiago Rodriguez
- Vanessa Vega
- Juan José Herrera

---

## Video de Evidencia

🎬 [Ver video en YouTube](https://youtube.com/watch?v=XXXXXXX)

---

## Endpoints Disponibles

### Auth
- `POST /api/auth/login` — Iniciar sesión
- `POST /api/auth/{id}/cambiar-password` — Cambiar contraseña

### Usuarios
- `GET /api/usuarios` — Listar usuarios (filtros: rolId, activo, busqueda)
- `GET /api/usuarios/{id}` — Obtener usuario por ID
- `GET /api/usuarios/estadisticas` — Dashboard de estadísticas
- `POST /api/usuarios` — Registrar usuario
- `PUT /api/usuarios/{id}` — Actualizar perfil
- `DELETE /api/usuarios/{id}` — Desactivar usuario
- `PATCH /api/usuarios/{id}/reactivar` — Reactivar usuario

### Roles
- `GET /api/roles` — Listar roles
- `GET /api/roles/{id}` — Obtener rol
- `POST /api/roles` — Crear rol
- `PUT /api/roles/{id}` — Actualizar rol
- `DELETE /api/roles/{id}` — Desactivar rol

---

## Ejecución con Docker

```bash
# Construir imagen
docker build -t gestion-usuarios-api:v1 .

# Ejecutar contenedor
docker run -d -p 5000:8080 --name gestion-api gestion-usuarios-api:v1

# Acceder a Swagger
# http://localhost:5000/swagger
```

## Ejecución con Kubernetes

```bash
# Asegurar que Kubernetes está habilitado en Docker Desktop

# Aplicar manifiestos
kubectl apply -f k8s-deployment.yaml

# Verificar
kubectl get pods
kubectl get svc

# Acceder a Swagger
# http://localhost:30080/swagger
```

---

## Usuarios de Prueba

| Usuario | Contraseña | Rol |
|---|---|---|
| admin | Admin123 | Administrador |
| mgarcia | Maria123 | Editor |

---

## Tecnologías

- ASP.NET Core 8.0
- Entity Framework Core (InMemory)
- Docker
- Kubernetes
- Swagger / OpenAPI
