# Guía para explicar el trabajo

1. **La base primero.** Ejecutamos el SQL entregado. EF Core lee las tablas y genera entidades y `EventosPeruDbContext`; eso es Database-First. No creamos una base distinta desde clases.
2. **Separación.** La API recibe HTTP; los DTO validan la entrada; `AuthService` aplica las reglas; el contexto persiste. La inyección de dependencias entrega los objetos sin crearlos dentro del controlador.
3. **Registro.** Validamos datos y correo único, calculamos un hash de contraseña y guardamos usuario e intereses en una transacción. Si falla, no queda un usuario a medias. No hay campo de entrada para ser administrador.
4. **Login.** Buscamos el correo y comprobamos estado y hash. Generamos un JWT con identidad y permisos, firmado con una clave privada del entorno. Swagger permite pegarlo para acceder a `/api/auth/me`.
5. **Recuperación.** El endpoint responde igual para correo conocido y desconocido. Se entrega un token aleatorio, pero se guarda sólo el hash. El UPDATE condicional permite consumirlo una vez y antes de su caducidad. Después se guarda la nueva contraseña y se invalidan otros tokens.
6. **Demostración.** Crear cuenta con intereses → intentar duplicarla → login incorrecto → login correcto → `/me` con JWT → solicitar recuperación → abrir enlace de Outbox → cambiar contraseña → comprobar que falla la antigua y funciona la nueva → intentar reutilizar el enlace.
7. **Pruebas y límites.** SQLite permite probar HTTP y reglas rápidamente. SQL Server real de CI verifica el SQL y genera el modelo completo. SMTP en desarrollo es un buzón local de prueba. No están hechas las otras historias del catálogo, compras o chatbot.

Preguntas frecuentes:

- **¿Hash o cifrado?** Hash de una sola vía para contraseñas; se verifica con `PasswordHasher`, no se descifra.
- **¿JWT es sesión en la base?** No. Es un documento firmado que la API valida con emisor, audiencia y vencimiento. Un JWT ya emitido vive hasta su vencimiento (60 minutos); no hay lista de revocación implementada.
- **¿Por qué DTO?** Evita exponer campos internos y evita recibir permisos administrativos desde el cliente.
- **¿Por qué token con hash?** Una filtración de la tabla no revela directamente el enlace utilizable.
- **¿Por qué transacción?** Usuario e intereses se guardan juntos; el consumo del token y el cambio de contraseña también.
