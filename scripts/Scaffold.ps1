$ErrorActionPreference = "Stop"
if (-not $env:ConnectionStrings__EventosPeruIA) { throw "Define ConnectionStrings__EventosPeruIA para una base EventosPeruIA ya creada con Database/EventosPeruIA.sql." }
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw "No se pudo restaurar dotnet-ef" }
dotnet ef dbcontext scaffold $env:ConnectionStrings__EventosPeruIA Microsoft.EntityFrameworkCore.SqlServer --project src/EventosPeruIA.Domain --startup-project src/EventosPeruIA.Api --context EventosPeruDbContext --output-dir Core/Entities --context-dir Infrastructure/Data --namespace EventosPeruIA.Domain.Core.Entities --context-namespace EventosPeruIA.Domain.Infrastructure.Data --use-database-names --no-pluralize --no-onconfiguring --force
if ($LASTEXITCODE -ne 0) { throw "Scaffolding fallido" }
