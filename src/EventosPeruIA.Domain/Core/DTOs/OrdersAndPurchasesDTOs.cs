using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace EventosPeruIA.CORE.Core.DTOs;

public class ServiceOrderCreateDto
{
    [Required]
    public string Customer { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;
}

public class ServiceOrderUpdateDto
{
    public string? Description { get; set; }
    public string? AssignedTo { get; set; }
    public string? Status { get; set; }
}

public class ServiceOrderDto
{
    public int Id { get; set; }
    public string Customer { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? AssignedTo { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PurchaseRequestDto
{
    [Required]
    public int CompradorId { get; set; }

    [Required]
    public int EventoId { get; set; }

    [Required]
    [Range(1, 1000)]
    public int CantidadEntradas { get; set; }
}

public class PurchaseResponseDto
{
    public int CompraId { get; set; }
    public int EventoId { get; set; }
    public int CompradorId { get; set; }
    public int CantidadEntradas { get; set; }
    public decimal MontoTotal { get; set; }
    public string CodigoTransaccion { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public DateTime FechaCompra { get; set; }
}