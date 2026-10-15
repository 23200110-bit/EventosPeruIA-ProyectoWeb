# EventosPeruIA — base y autenticación

Solución académica para Visual Studio 2026 actualizado (18.x), .NET 10 y SQL Server. Implementa la parte de infraestructura y autenticación del trabajo: US001, US002 y recuperación US018. El resto del backlog no está implementado todavía.

## Ejecutar en Windows

1. Instala Visual Studio 2026 con ASP.NET y desarrollo web, SDK .NET 10 y SQL Server Express LocalDB (o SQL Server).
2. En SSMS o Visual Studio ejecuta **una sola vez, sobre una base nueva**, `Database/EventosPeruIA.sql`. Es el archivo original recibido, sin cambios; las tablas no son idempotentes. No lo ejecutes sobre una base con datos existentes.
3. Abre `EventosPeruIA.sln`. Establece `EventosPeruIA.Api` como proyecto de inicio.
4. Desde la raíz, configura una clave JWT privada:

```powershell
$bytes = New-Object byte[] 48
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
dotnet user-secrets set "Jwt:Key" ([Convert]::ToBase64String($bytes)) --project src/EventosPeruIA.Api
```

5. La conexión predeterminada usa `(localdb)\MSSQLLocalDB`. Para otra instancia, guarda `ConnectionStrings:EventosPeruIA` mediante `dotnet user-secrets set` en el mismo proyecto. No subas contraseñas ni archivos locales a Git.
6. Ejecuta:

```powershell
dotnet restore
dotnet run --project src/EventosPeruIA.Api
```

Abre **http://localhost:5080** para la pantalla de acceso y **http://localhost:5080/swagger** para probar la API. En Visual Studio usa el perfil `http` (Development).

## Qué está implementado

- Registro con nombres, apellidos, correo único, contraseña de al menos ocho caracteres y confirmación; intereses opcionales válidos. El usuario nunca puede darse permisos de administrador.
- Contraseñas con `PasswordHasher` de ASP.NET Core (PBKDF2 con sal). No se guardan ni devuelven en claro.
- Login de usuarios activos, JWT firmado y validado con `UsuarioId`, `Correo` y `EsAdmin`; endpoint protegido `/api/auth/me`.
- Recuperación con respuesta genérica, token aleatorio, sólo su SHA-256 en la base, caducidad de 30 minutos y uso único. El cambio invalida otros tokens pendientes y la contraseña anterior deja de servir.
- Interfaz con validación, categorías, registro, login, recuperación y cambio de contraseña. El botón se habilita sólo si el formulario es válido.
- Swagger con Bearer y límite de solicitudes en autenticación.

En **Development** no se manda correo real: el enlace se guarda en `src/EventosPeruIA.Api/App_Data/Outbox/*.txt` (ignorado por Git). Ábrelo para completar la demostración. En producción se exige SMTP: configura `Smtp__Host`, `Smtp__Port`, `Smtp__User`, `Smtp__Password`, `Smtp__From`, `Recovery__PublicUrl` y HTTPS; no se ha contratado ni activado un servicio de correo.

## Database-First

La fuente de verdad es el SQL original. Para regenerar las entidades y contexto desde tu SQL Server:

```powershell
$env:ConnectionStrings__EventosPeruIA = "Server=(localdb)\MSSQLLocalDB;Database=EventosPeruIA;Integrated Security=True;TrustServerCertificate=True"
./scripts/Scaffold.ps1
```

El script usa EF Core 10.0.13, preserva nombres de base, no escribe la conexión en código y reemplaza únicamente el modelo generado. Los DTO y servicios están separados. No se usan migraciones ni `EnsureCreated` para sustituir el SQL original.

El workflow `validacion.yml` ejecuta el SQL original en un **SQL Server 2022 desechable**, hace scaffolding real, recompila y vuelve a correr las pruebas. El artefacto contiene el modelo generado y los resultados. La clave de SQL de ese workflow es exclusivamente sintética para la instancia efímera de CI; no pertenece a ningún servidor real.

## Probar

```powershell
dotnet test EventosPeruIA.sln
```

Las pruebas HTTP usan SQLite en memoria para verificar reglas de autenticación; **no equivalen** a ejecutar el SQL de SQL Server. El workflow cubre por separado SQL Server y scaffolding real.

## Organización

- `src/EventosPeruIA.Api`: controladores, JWT, correo, Swagger, configuración y pantalla.
- `src/EventosPeruIA.Domain/Core`: entidades y DTO, contratos.
- `src/EventosPeruIA.Domain/Infrastructure/Data`: contexto EF Database-First.
- `src/EventosPeruIA.Domain/Infrastructure/Services`: reglas de autenticación.
- `Database`: SQL original del trabajo.
- `tests`: pruebas de integración HTTP.
- `docs/DEFENSA.md`: guía para explicar el código.

Las invitaciones de los cuatro compañeros y del docente requieren sus usuarios de GitHub; no se inventan identidades ni se modifican permisos sin identificarlos.

Documentación: [EF Database-First](https://learn.microsoft.com/ef/core/managing-schemas/scaffolding/), [secretos en desarrollo](https://learn.microsoft.com/aspnet/core/security/app-secrets).
