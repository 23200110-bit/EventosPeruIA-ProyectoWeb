/* ============================================================================
   EventosPeruIA - Base de Datos Consolidada y Definitiva (SQL Server)
   Curso: Desarrollo de Aplicaciones Web - Universidad ESAN
   Cubre el 100% del Product Backlog (US-001 a US-019) y Reglas RN-01 a RN-10
   ============================================================================ */

IF DB_ID('EventosPeruIA') IS NULL
BEGIN
    CREATE DATABASE EventosPeruIA;
END
GO

USE EventosPeruIA;
GO

/* ============================================================================
   1. CATÁLOGOS BASE
   ============================================================================ */

CREATE TABLE Categorias (
    CategoriaId     INT IDENTITY(1,1)      NOT NULL,
    Nombre          NVARCHAR(60)           NOT NULL,
    Descripcion     NVARCHAR(255)          NULL,
    Activo          BIT                    NOT NULL CONSTRAINT DF_Categorias_Activo DEFAULT (1),
    CONSTRAINT PK_Categorias PRIMARY KEY (CategoriaId),
    CONSTRAINT UQ_Categorias_Nombre UNIQUE (Nombre)
);
GO

CREATE TABLE MotivosCancelacion (
    MotivoCancelacionId INT IDENTITY(1,1)  NOT NULL,
    Codigo              VARCHAR(40)        NOT NULL,
    Descripcion         NVARCHAR(200)      NOT NULL,
    CONSTRAINT PK_MotivosCancelacion PRIMARY KEY (MotivoCancelacionId),
    CONSTRAINT UQ_MotivosCancelacion_Codigo UNIQUE (Codigo)
);
GO

/* ============================================================================
   2. USUARIOS, PERFIL Y SEGURIDAD (US-001, US-002, US-003, US-018)
   ============================================================================ */

CREATE TABLE Usuarios (
    UsuarioId           INT IDENTITY(1,1)     NOT NULL,
    Nombres             NVARCHAR(100)         NOT NULL,
    Apellidos           NVARCHAR(100)         NOT NULL,
    Correo              NVARCHAR(150)         NOT NULL,
    ContrasenaHash      NVARCHAR(255)         NOT NULL,
    EsAdministrador     BIT                   NOT NULL CONSTRAINT DF_Usuarios_EsAdmin DEFAULT (0),
    Estado              VARCHAR(20)           NOT NULL CONSTRAINT DF_Usuarios_Estado DEFAULT ('Activo'),
    CiudadUbicacion     NVARCHAR(100)         NULL,
    Latitud             DECIMAL(9,6)          NULL,
    Longitud            DECIMAL(9,6)          NULL,
    FechaRegistro       DATETIME2             NOT NULL CONSTRAINT DF_Usuarios_FechaRegistro DEFAULT (SYSUTCDATETIME()),
    FechaActualizacion  DATETIME2             NULL,
    CONSTRAINT PK_Usuarios PRIMARY KEY (UsuarioId),
    CONSTRAINT UQ_Usuarios_Correo UNIQUE (Correo),
    CONSTRAINT CK_Usuarios_Estado CHECK (Estado IN ('Activo', 'Bloqueado', 'Inactivo'))
);
GO

CREATE TABLE UsuarioIntereses (
    UsuarioId     INT     NOT NULL,
    CategoriaId   INT     NOT NULL,
    CONSTRAINT PK_UsuarioIntereses PRIMARY KEY (UsuarioId, CategoriaId),
    CONSTRAINT FK_UsuarioIntereses_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE,
    CONSTRAINT FK_UsuarioIntereses_Categoria FOREIGN KEY (CategoriaId) REFERENCES Categorias(CategoriaId) ON DELETE CASCADE
);
GO

CREATE TABLE TokensRecuperacionContrasena (
    TokenId         UNIQUEIDENTIFIER   NOT NULL CONSTRAINT DF_Tokens_Id DEFAULT (NEWID()),
    UsuarioId       INT                NOT NULL,
    TokenHash       NVARCHAR(255)      NOT NULL,
    FechaCreacion   DATETIME2          NOT NULL CONSTRAINT DF_Tokens_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    FechaExpiracion DATETIME2          NOT NULL,
    Utilizado       BIT                NOT NULL CONSTRAINT DF_Tokens_Utilizado DEFAULT (0),
    FechaUso        DATETIME2          NULL,
    CONSTRAINT PK_TokensRecuperacion PRIMARY KEY (TokenId),
    CONSTRAINT FK_Tokens_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId) ON DELETE CASCADE
);
GO

/* ============================================================================
   3. GESTIÓN DE EVENTOS E IA / RAG (US-004 a US-009, US-017)
   ============================================================================ */

CREATE TABLE Eventos (
    EventoId            INT IDENTITY(1,1)   NOT NULL,
    OrganizadorId       INT                 NOT NULL,
    CategoriaId         INT                 NOT NULL,
    Nombre              NVARCHAR(150)       NOT NULL,
    Descripcion         NVARCHAR(MAX)       NOT NULL,
    FechaInicio         DATETIME2           NOT NULL,
    FechaFin            DATETIME2           NULL,
    Direccion           NVARCHAR(250)       NOT NULL,
    Latitud             DECIMAL(9,6)        NULL,
    Longitud            DECIMAL(9,6)        NULL,
    Aforo               INT                 NOT NULL,
    Precio              DECIMAL(10,2)       NOT NULL,
    Estado              VARCHAR(20)         NOT NULL CONSTRAINT DF_Eventos_Estado DEFAULT ('Borrador'),
    EstadoModeracion    VARCHAR(20)         NOT NULL CONSTRAINT DF_Eventos_EstadoModeracion DEFAULT ('Aprobado'),
    FechaCreacion       DATETIME2           NOT NULL CONSTRAINT DF_Eventos_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    FechaPublicacion    DATETIME2           NULL,
    FechaActualizacion  DATETIME2           NULL,
    VersionFila         ROWVERSION, -- Concurrencia optimista para EF Core
    CONSTRAINT PK_Eventos PRIMARY KEY (EventoId),
    CONSTRAINT FK_Eventos_Organizador FOREIGN KEY (OrganizadorId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT FK_Eventos_Categoria FOREIGN KEY (CategoriaId) REFERENCES Categorias(CategoriaId),
    CONSTRAINT CK_Eventos_Aforo CHECK (Aforo > 0),
    CONSTRAINT CK_Eventos_Precio CHECK (Precio >= 0),
    CONSTRAINT CK_Eventos_Estado CHECK (Estado IN ('Borrador', 'Publicado', 'Finalizado', 'Cancelado', 'Suspendido')),
    CONSTRAINT CK_Eventos_EstadoModeracion CHECK (EstadoModeracion IN ('PendienteRevision', 'Aprobado', 'Rechazado', 'Suspendido'))
);
GO

CREATE TABLE EventosCancelacion (
    EventoCancelacionId       INT IDENTITY(1,1) NOT NULL,
    EventoId                  INT               NOT NULL,
    MotivoCancelacionId       INT               NOT NULL,
    DescripcionAdicional      NVARCHAR(500)     NOT NULL,
    CanceladoPorId            INT               NOT NULL,
    CantidadEntradasAfectadas INT               NOT NULL CONSTRAINT DF_EventosCancelacion_Cantidad DEFAULT (0),
    FechaHora                 DATETIME2         NOT NULL CONSTRAINT DF_EventosCancelacion_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_EventosCancelacion PRIMARY KEY (EventoCancelacionId),
    CONSTRAINT UQ_EventosCancelacion_Evento UNIQUE (EventoId),
    CONSTRAINT FK_EventosCancelacion_Evento FOREIGN KEY (EventoId) REFERENCES Eventos(EventoId),
    CONSTRAINT FK_EventosCancelacion_Motivo FOREIGN KEY (MotivoCancelacionId) REFERENCES MotivosCancelacion(MotivoCancelacionId),
    CONSTRAINT FK_EventosCancelacion_Usuario FOREIGN KEY (CanceladoPorId) REFERENCES Usuarios(UsuarioId)
);
GO

CREATE TABLE EventosEmbeddings (
    EmbeddingId      INT IDENTITY(1,1)  NOT NULL,
    EventoId         INT                NOT NULL,
    ContenidoTexto   NVARCHAR(MAX)      NOT NULL,
    VectorEmbedding  VARBINARY(MAX)     NOT NULL,
    EmbeddingJson    NVARCHAR(MAX)      NULL, -- Soporte híbrido JSON/Vector
    ModeloUsado      VARCHAR(100)       NOT NULL,
    Version          INT                NOT NULL CONSTRAINT DF_Embeddings_Version DEFAULT (1),
    Vigente          BIT                NOT NULL CONSTRAINT DF_Embeddings_Vigente DEFAULT (1),
    FechaGeneracion  DATETIME2          NOT NULL CONSTRAINT DF_Embeddings_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_EventosEmbeddings PRIMARY KEY (EmbeddingId),
    CONSTRAINT FK_EventosEmbeddings_Evento FOREIGN KEY (EventoId) REFERENCES Eventos(EventoId) ON DELETE CASCADE
);
GO

CREATE TABLE UsuarioEventoInteraccion (
    InteraccionId   BIGINT IDENTITY(1,1) NOT NULL,
    UsuarioId       INT                  NOT NULL,
    EventoId        INT                  NOT NULL,
    TipoInteraccion VARCHAR(20)          NOT NULL,
    FechaHora       DATETIME2            NOT NULL CONSTRAINT DF_Interaccion_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_UsuarioEventoInteraccion PRIMARY KEY (InteraccionId),
    CONSTRAINT FK_Interaccion_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT FK_Interaccion_Evento FOREIGN KEY (EventoId) REFERENCES Eventos(EventoId),
    CONSTRAINT CK_Interaccion_Tipo CHECK (TipoInteraccion IN ('Vista', 'Busqueda', 'Compra', 'Favorito'))
);
GO

/* ============================================================================
   4. TRANSACCIONES, ENTRADAS Y CHECK-IN (US-010, US-011, US-012, US-019)
   ============================================================================ */

CREATE TABLE Compras (
    CompraId          INT IDENTITY(1,1)  NOT NULL,
    CompradorId       INT                NOT NULL,
    EventoId          INT                NOT NULL,
    CantidadEntradas  INT                NOT NULL,
    MontoTotal        DECIMAL(10,2)      NOT NULL,
    CodigoTransaccion VARCHAR(50)        NULL,
    Estado            VARCHAR(20)        NOT NULL CONSTRAINT DF_Compras_Estado DEFAULT ('Confirmada'),
    FechaCompra       DATETIME2          NOT NULL CONSTRAINT DF_Compras_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Compras PRIMARY KEY (CompraId),
    CONSTRAINT FK_Compras_Comprador FOREIGN KEY (CompradorId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT FK_Compras_Evento FOREIGN KEY (EventoId) REFERENCES Eventos(EventoId),
    CONSTRAINT CK_Compras_Cantidad CHECK (CantidadEntradas > 0),
    CONSTRAINT CK_Compras_Monto CHECK (MontoTotal >= 0),
    CONSTRAINT CK_Compras_Estado CHECK (Estado IN ('Confirmada', 'Anulada', 'Reembolsada'))
);
GO

CREATE TABLE Entradas (
    EntradaId        UNIQUEIDENTIFIER   NOT NULL CONSTRAINT DF_Entradas_Id DEFAULT (NEWID()),
    CompraId         INT                NOT NULL,
    EventoId         INT                NOT NULL,
    CompradorId      INT                NOT NULL,
    PrecioPagado     DECIMAL(10,2)      NOT NULL,
    Estado           VARCHAR(20)        NOT NULL CONSTRAINT DF_Entradas_Estado DEFAULT ('Valida'),
    EstadoReembolso  VARCHAR(20)        NULL,
    FechaEmision     DATETIME2          NOT NULL CONSTRAINT DF_Entradas_FechaEmision DEFAULT (SYSUTCDATETIME()),
    FechaUso         DATETIME2          NULL,
    FechaAnulacion   DATETIME2          NULL,
    CONSTRAINT PK_Entradas PRIMARY KEY (EntradaId),
    CONSTRAINT FK_Entradas_Compra FOREIGN KEY (CompraId) REFERENCES Compras(CompraId),
    CONSTRAINT FK_Entradas_Evento FOREIGN KEY (EventoId) REFERENCES Eventos(EventoId),
    CONSTRAINT FK_Entradas_Comprador FOREIGN KEY (CompradorId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT CK_Entradas_Precio CHECK (PrecioPagado >= 0),
    CONSTRAINT CK_Entradas_Estado CHECK (Estado IN ('Valida', 'Utilizada', 'Anulada')),
    CONSTRAINT CK_Entradas_EstadoReembolso CHECK (EstadoReembolso IN ('PendienteReembolso', 'Reembolsada') OR EstadoReembolso IS NULL)
);
GO

ALTER TABLE Entradas
    ADD CodigoCorto AS ('TCK-' + UPPER(LEFT(CONVERT(VARCHAR(36), EntradaId), 8))) PERSISTED;
GO

CREATE TABLE CheckIns (
    CheckInId       INT IDENTITY(1,1)  NOT NULL,
    EntradaId       UNIQUEIDENTIFIER   NOT NULL,
    ValidadoPorId   INT                NOT NULL,
    MetodoIngreso   VARCHAR(20)        NOT NULL,
    Resultado       VARCHAR(20)        NOT NULL,
    MotivoRechazo   NVARCHAR(200)      NULL,
    FechaHora       DATETIME2          NOT NULL CONSTRAINT DF_CheckIns_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_CheckIns PRIMARY KEY (CheckInId),
    CONSTRAINT FK_CheckIns_Entrada FOREIGN KEY (EntradaId) REFERENCES Entradas(EntradaId),
    CONSTRAINT FK_CheckIns_Validador FOREIGN KEY (ValidadoPorId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT CK_CheckIns_Metodo CHECK (MetodoIngreso IN ('QR', 'Manual')),
    CONSTRAINT CK_CheckIns_Resultado CHECK (Resultado IN ('Aprobado', 'Rechazado'))
);
GO

-- RN-05 / US-012: Previene dobles validaciones exitosas en la BD
CREATE UNIQUE INDEX UX_CheckIns_EntradaAprobada
    ON CheckIns(EntradaId)
    WHERE Resultado = 'Aprobado';
GO

/* ============================================================================
   5. RESEÑAS Y NOTIFICACIONES (US-013, US-014)
   ============================================================================ */

CREATE TABLE Resenas (
    ResenaId       INT IDENTITY(1,1)  NOT NULL,
    EntradaId      UNIQUEIDENTIFIER   NOT NULL,
    EventoId       INT                NOT NULL,
    UsuarioId      INT                NOT NULL,
    Calificacion   TINYINT            NOT NULL,
    Comentario     NVARCHAR(1000)     NULL,
    Estado         VARCHAR(20)        NOT NULL CONSTRAINT DF_Resenas_Estado DEFAULT ('Activa'),
    FechaCreacion  DATETIME2          NOT NULL CONSTRAINT DF_Resenas_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Resenas PRIMARY KEY (ResenaId),
    CONSTRAINT FK_Resenas_Entrada FOREIGN KEY (EntradaId) REFERENCES Entradas(EntradaId),
    CONSTRAINT FK_Resenas_Evento FOREIGN KEY (EventoId) REFERENCES Eventos(EventoId),
    CONSTRAINT FK_Resenas_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT CK_Resenas_Calificacion CHECK (Calificacion BETWEEN 1 AND 5),
    CONSTRAINT CK_Resenas_Estado CHECK (Estado IN ('Activa', 'Moderada')),
    CONSTRAINT UQ_Resenas_UsuarioEvento UNIQUE (UsuarioId, EventoId)
);
GO

CREATE TABLE Notificaciones (
    NotificacionId  BIGINT IDENTITY(1,1) NOT NULL,
    UsuarioId       INT                  NOT NULL,
    EventoId        INT                  NULL,
    Tipo            VARCHAR(30)          NOT NULL,
    Mensaje         NVARCHAR(500)        NOT NULL,
    Leida           BIT                  NOT NULL CONSTRAINT DF_Notificaciones_Leida DEFAULT (0),
    FechaCreacion   DATETIME2            NOT NULL CONSTRAINT DF_Notificaciones_Fecha DEFAULT (SYSUTCDATETIME()),
    FechaLectura    DATETIME2            NULL,
    CONSTRAINT PK_Notificaciones PRIMARY KEY (NotificacionId),
    CONSTRAINT FK_Notificaciones_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT FK_Notificaciones_Evento FOREIGN KEY (EventoId) REFERENCES Eventos(EventoId),
    CONSTRAINT CK_Notificaciones_Tipo CHECK (Tipo IN ('ConfirmacionCompra', 'Recordatorio', 'CambioEvento', 'CancelacionEvento'))
);
GO

/* ============================================================================
   6. MODERACIÓN Y AUDITORÍA (US-016)
   ============================================================================ */

CREATE TABLE AccionesModeracion (
    AccionId          BIGINT IDENTITY(1,1) NOT NULL,
    AdministradorId   INT                  NOT NULL,
    TipoAccion        VARCHAR(30)          NOT NULL,
    EntidadTipo       VARCHAR(20)          NOT NULL,
    EntidadId         INT                  NOT NULL,
    Motivo            NVARCHAR(300)        NULL,
    FechaHora         DATETIME2            NOT NULL CONSTRAINT DF_AccionesModeracion_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_AccionesModeracion PRIMARY KEY (AccionId),
    CONSTRAINT FK_AccionesModeracion_Admin FOREIGN KEY (AdministradorId) REFERENCES Usuarios(UsuarioId),
    CONSTRAINT CK_AccionesModeracion_Tipo CHECK (TipoAccion IN
        ('BloquearUsuario', 'ReactivarUsuario', 'AprobarEvento', 'RechazarEvento', 'RetirarEvento', 'SuspenderEvento')),
    CONSTRAINT CK_AccionesModeracion_Entidad CHECK (EntidadTipo IN ('Usuario', 'Evento'))
);
GO

/* ============================================================================
   7. ASISTENTE CONVERSACIONAL Y CHATBOT (US-017)
   ============================================================================ */

CREATE TABLE ConversacionesChatbot (
    ConversacionId  INT IDENTITY(1,1) NOT NULL,
    UsuarioId       INT               NOT NULL,
    FechaInicio     DATETIME2         NOT NULL CONSTRAINT DF_Conversaciones_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ConversacionesChatbot PRIMARY KEY (ConversacionId),
    CONSTRAINT FK_Conversaciones_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId)
);
GO

CREATE TABLE MensajesChatbot (
    MensajeId       BIGINT IDENTITY(1,1) NOT NULL,
    ConversacionId  INT                  NOT NULL,
    Emisor          VARCHAR(15)          NOT NULL,
    Contenido       NVARCHAR(MAX)        NOT NULL,
    FechaHora       DATETIME2            NOT NULL CONSTRAINT DF_Mensajes_Fecha DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_MensajesChatbot PRIMARY KEY (MensajeId),
    CONSTRAINT FK_Mensajes_Conversacion FOREIGN KEY (ConversacionId) REFERENCES ConversacionesChatbot(ConversacionId),
    CONSTRAINT CK_Mensajes_Emisor CHECK (Emisor IN ('Usuario', 'Asistente'))
);
GO

/* ============================================================================
   8. ÍNDICES DE RENDIMIENTO (US-005, US-006, US-015)
   ============================================================================ */

CREATE INDEX IX_Eventos_Estado_FechaInicio ON Eventos(Estado, FechaInicio);
CREATE INDEX IX_Eventos_CategoriaId ON Eventos(CategoriaId);
CREATE INDEX IX_Entradas_EventoId_Estado ON Entradas(EventoId, Estado);
CREATE INDEX IX_Notificaciones_UsuarioId_Leida ON Notificaciones(UsuarioId, Leida);
CREATE INDEX IX_UsuarioEventoInteraccion_UsuarioId ON UsuarioEventoInteraccion(UsuarioId, EventoId);
GO

/* ============================================================================
   9. VISTAS PARA REPORTES Y RECURSOS (US-006, US-015)
   ============================================================================ */

CREATE VIEW vw_DisponibilidadEventos AS
SELECT
    e.EventoId,
    e.Nombre,
    e.Aforo,
    COUNT(CASE WHEN t.Estado IN ('Valida', 'Utilizada') THEN 1 END) AS EntradasVendidas,
    e.Aforo - COUNT(CASE WHEN t.Estado IN ('Valida', 'Utilizada') THEN 1 END) AS CuposDisponibles
FROM Eventos e
LEFT JOIN Entradas t ON t.EventoId = e.EventoId
GROUP BY e.EventoId, e.Nombre, e.Aforo;
GO

CREATE VIEW vw_VentasPorEvento AS
SELECT
    e.EventoId,
    e.OrganizadorId,
    e.Nombre AS NombreEvento,
    COUNT(CASE WHEN t.Estado IN ('Valida', 'Utilizada') THEN 1 END)             AS EntradasVendidas,
    SUM(CASE WHEN t.Estado IN ('Valida', 'Utilizada') THEN t.PrecioPagado ELSE 0 END) AS IngresosConfirmados,
    COUNT(CASE WHEN t.Estado = 'Anulada' THEN 1 END)                            AS EntradasAnuladas,
    SUM(CASE WHEN t.Estado = 'Anulada' THEN t.PrecioPagado ELSE 0 END)          AS MontoPendienteReembolso
FROM Eventos e
LEFT JOIN Entradas t ON t.EventoId = e.EventoId
GROUP BY e.EventoId, e.OrganizadorId, e.Nombre;
GO

CREATE VIEW vw_VentasPorFecha AS
SELECT
    t.EventoId,
    CAST(t.FechaEmision AS DATE) AS Fecha,
    COUNT(*)                                                       AS EntradasVendidasEseDia,
    SUM(CASE WHEN t.Estado IN ('Valida', 'Utilizada') THEN t.PrecioPagado ELSE 0 END) AS IngresosEseDia
FROM Entradas t
WHERE t.Estado IN ('Valida', 'Utilizada')
GROUP BY t.EventoId, CAST(t.FechaEmision AS DATE);
GO

/* ============================================================================
   10. LÓGICA TRANSACCIONAL Y PROCEDIMIENTOS ALMACENADOS
   ============================================================================ */

CREATE PROCEDURE sp_ComprarEntradas
    @CompradorId      INT,
    @EventoId         INT,
    @CantidadEntradas INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @Aforo INT, @Precio DECIMAL(10,2), @Estado VARCHAR(20);
    SELECT
        @Aforo  = Aforo,
        @Precio = Precio,
        @Estado = Estado
    FROM Eventos WITH (UPDLOCK, HOLDLOCK)
    WHERE EventoId = @EventoId;

    IF @Estado IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50001, 'El evento no existe.', 1;
    END

    IF @Estado <> 'Publicado'
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50002, 'El evento no está disponible para la venta de entradas.', 1;
    END

    DECLARE @Vendidas INT;
    SELECT @Vendidas = COUNT(*)
    FROM Entradas
    WHERE EventoId = @EventoId AND Estado IN ('Valida', 'Utilizada');

    IF (@Vendidas + @CantidadEntradas) > @Aforo
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50003, 'No hay cupos suficientes disponibles para esta compra.', 1;
    END

    DECLARE @CompraId INT;
    INSERT INTO Compras (CompradorId, EventoId, CantidadEntradas, MontoTotal)
    VALUES (@CompradorId, @EventoId, @CantidadEntradas, @Precio * @CantidadEntradas);

    SET @CompraId = SCOPE_IDENTITY();

    ;WITH Numeros AS (
        SELECT TOP (@CantidadEntradas) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
        FROM sys.all_objects
    )
    INSERT INTO Entradas (CompraId, EventoId, CompradorId, PrecioPagado)
    SELECT @CompraId, @EventoId, @CompradorId, @Precio
    FROM Numeros;

    INSERT INTO Notificaciones (UsuarioId, EventoId, Tipo, Mensaje)
    VALUES (@CompradorId, @EventoId, 'ConfirmacionCompra', 'Tu compra de entradas fue confirmada.');

    INSERT INTO UsuarioEventoInteraccion (UsuarioId, EventoId, TipoInteraccion)
    VALUES (@CompradorId, @EventoId, 'Compra');

    COMMIT TRANSACTION;

    SELECT EntradaId, CodigoCorto, Estado, FechaEmision
    FROM Entradas
    WHERE CompraId = @CompraId;
END
GO

CREATE PROCEDURE sp_CancelarEvento
    @EventoId               INT,
    @CanceladoPorId         INT,
    @MotivoCancelacionId    INT,
    @DescripcionAdicional   NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @Estado VARCHAR(20), @FechaInicio DATETIME2, @Organizador INT;
    SELECT
        @Estado      = Estado,
        @FechaInicio = FechaInicio,
        @Organizador = OrganizadorId
    FROM Eventos WITH (UPDLOCK, HOLDLOCK)
    WHERE EventoId = @EventoId;

    IF @Estado IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50011, 'El evento no existe.', 1;
    END

    IF @Estado NOT IN ('Borrador', 'Publicado')
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50012, 'Solo se pueden cancelar eventos en Borrador o Publicado.', 1;
    END

    IF @FechaInicio <= SYSUTCDATETIME()
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50013, 'El evento ya inició; no puede cancelarse.', 1;
    END

    DECLARE @EntradasAfectadas INT;
    SELECT @EntradasAfectadas = COUNT(*)
    FROM Entradas
    WHERE EventoId = @EventoId AND Estado = 'Valida';

    UPDATE Eventos
    SET Estado = 'Cancelado', FechaActualizacion = SYSUTCDATETIME()
    WHERE EventoId = @EventoId;

    INSERT INTO EventosCancelacion (EventoId, MotivoCancelacionId, DescripcionAdicional, CanceladoPorId, CantidadEntradasAfectadas)
    VALUES (@EventoId, @MotivoCancelacionId, @DescripcionAdicional, @CanceladoPorId, @EntradasAfectadas);

    UPDATE Entradas
    SET Estado = 'Anulada',
        EstadoReembolso = 'PendienteReembolso',
        FechaAnulacion = SYSUTCDATETIME()
    WHERE EventoId = @EventoId AND Estado = 'Valida';

    INSERT INTO Notificaciones (UsuarioId, EventoId, Tipo, Mensaje)
    SELECT DISTINCT CompradorId, @EventoId, 'CancelacionEvento',
           'El evento fue cancelado. Tu(s) entrada(s) fueron anuladas y quedan pendientes de reembolso.'
    FROM Entradas
    WHERE EventoId = @EventoId AND EstadoReembolso = 'PendienteReembolso';

    UPDATE EventosEmbeddings
    SET Vigente = 0
    WHERE EventoId = @EventoId AND Vigente = 1;

    COMMIT TRANSACTION;

    SELECT @EntradasAfectadas AS EntradasAfectadas;
END
GO

/* ============================================================================
   11. DATOS SEMILLA
   ============================================================================ */

INSERT INTO Categorias (Nombre, Descripcion) VALUES
    (N'Conciertos', N'Música en vivo y festivales'),
    (N'Talleres', N'Capacitaciones y workshops prácticos'),
    (N'Ferias', N'Exposiciones culturales y gastronómicas'),
    (N'Charlas', N'Conferencias académicas y networking'),
    (N'Deportes', N'Eventos deportivos y competencias');
GO

INSERT INTO MotivosCancelacion (Codigo, Descripcion) VALUES
    ('FUERZA_MAYOR',      N'Fuerza mayor'),
    ('ORDEN_AUTORIDAD',   N'Orden o restricción de una autoridad'),
    ('PROBLEMA_RECINTO',  N'Problemas en el recinto o local del evento'),
    ('AUSENCIA_ARTISTA',  N'Imposibilidad del artista o expositor principal'),
    ('CLIMA_ADVERSO',     N'Clima adverso'),
    ('SEGURIDAD',         N'Razones de seguridad'),
    ('OTRO',              N'Otro motivo justificado');
GO