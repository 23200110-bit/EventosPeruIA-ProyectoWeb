using System;
using System.Collections.Generic;
using System.Text;

namespace EventosPeruIA.CORE.Core.DTOs;

public class ValidateCheckInRequestDTO
{
    public int EventoId { get; set; }
    // Puede recibir el Guid/UUID del QR o el código corto (ej. TCK-ABCD1234)
    public string CodigoTicket { get; set; } = string.Empty;
    public string MetodoIngreso { get; set; } = "QR"; // QR o Manual
    public int ValidadoPorId { get; set; } // Id del organizador que valida
}