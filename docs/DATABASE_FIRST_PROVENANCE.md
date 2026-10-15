# Procedencia del modelo Database-First

El modelo de `Core/Entities` y `Infrastructure/Data` fue generado por EF Core 8.0.32 desde un SQL Server 2022 real y desechable que ejecutó el SQL original.

- Ejecución: https://github.com/23200110-bit/EventosPeruIA-ProyectoWeb/actions/runs/38748835835
- Commit de entrada: fe4e86be456ff19f70bf5872466ea7c41b270b0f
- Artefacto: modelo-database-first-y-pruebas (11980690314).
- SHA-256 del ZIP: 209865bf762b09580706664518ae56bbeec21fc0844023d4362f6cc49af0916a
- Archivos C# del modelo: 20.
- SQL original SHA-256: 08e993374ef18cf4a08b3b0ab0153300bd0e47be2d8361d71258a2913ff01920.
- El workflow compiló y probó nuevamente después del scaffolding.

Los tests HTTP de reglas usan SQLite; además hay una prueba integrada de registro, login, recuperación y cambio de contraseña contra SQL Server real en CI. No se ejecutó SQL Server en la máquina ARM local ni se usó una instancia de producción.
