namespace AdoPetsBKD.Application.DTOs.Clinica;

public class CreateTicketDto
{
    public Guid CitaId { get; set; }
    public Guid? MascotaId { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeterinarioId { get; set; }
    public DateTime FechaProcedimiento { get; set; }
    public string NombreProcedimiento { get; set; } = string.Empty;
    public string? DescripcionProcedimiento { get; set; }
    public decimal CostoProcedimiento { get; set; }
    public decimal CostoInsumos { get; set; }
    public decimal CostoAdicional { get; set; }
    public decimal Descuento { get; set; }
    public string? Observaciones { get; set; }
    public string? Diagnostico { get; set; }
    public string? Tratamiento { get; set; }
    public string? MedicacionPrescrita { get; set; }
    public List<CreateTicketDetalleDto> Detalles { get; set; } = new();
}

public class CreateTicketDetalleDto
{
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string? Unidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public Guid? ItemInventarioId { get; set; }
    public int Tipo { get; set; } // TipoDetalleTicket
}

public class TicketDto
{
    public Guid Id { get; set; }
    public string NumeroTicket { get; set; } = string.Empty;
    public Guid CitaId { get; set; }
    public Guid? MascotaId { get; set; }
    public string? NombreMascota { get; set; }
    public Guid ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public Guid VeterinarioId { get; set; }
    public string NombreVeterinario { get; set; } = string.Empty;
    public DateTime FechaProcedimiento { get; set; }
    public string NombreProcedimiento { get; set; } = string.Empty;
    public string? DescripcionProcedimiento { get; set; }
    public decimal CostoProcedimiento { get; set; }
    public decimal CostoInsumos { get; set; }
    public decimal CostoAdicional { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal IVA { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public string? Diagnostico { get; set; }
    public string? Tratamiento { get; set; }
    public string? MedicacionPrescrita { get; set; }
    public int Estado { get; set; }
    public string EstadoNombre { get; set; } = string.Empty;
    public DateTime? FechaEntrega { get; set; }
    public Guid? PagoId { get; set; }
    public List<TicketDetalleDto> Detalles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class TicketDetalleDto
{
    public Guid Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string? Unidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public int Tipo { get; set; }
    public string TipoNombre { get; set; } = string.Empty;
}
