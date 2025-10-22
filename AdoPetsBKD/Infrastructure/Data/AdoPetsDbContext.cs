using Microsoft.EntityFrameworkCore;
using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Security;
using AdoPetsBKD.Domain.Entities.Mascotas;
using AdoPetsBKD.Domain.Entities.Clinica;
using AdoPetsBKD.Domain.Entities.HistorialClinico;
using AdoPetsBKD.Domain.Entities.Inventario;
using AdoPetsBKD.Domain.Entities.Donaciones;
using AdoPetsBKD.Domain.Entities.Servicios;
using AdoPetsBKD.Domain.Entities.Auditoria;

namespace AdoPetsBKD.Infrastructure.Data;

public class AdoPetsDbContext : DbContext
{
    public AdoPetsDbContext(DbContextOptions<AdoPetsDbContext> options) : base(options)
    {
    }

    #region Security
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<ConsentimientoPolitica> ConsentimientosPoliticas => Set<ConsentimientoPolitica>();
    public DbSet<Dispositivo> Dispositivos => Set<Dispositivo>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<PreferenciaNotificacion> PreferenciasNotificaciones => Set<PreferenciaNotificacion>();
    #endregion

    #region Mascotas
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<MascotaFoto> MascotasFotos => Set<MascotaFoto>();
    public DbSet<SolicitudAdopcion> SolicitudesAdopcion => Set<SolicitudAdopcion>();
    public DbSet<SolicitudAdopcionAdjunto> SolicitudesAdopcionAdjuntos => Set<SolicitudAdopcionAdjunto>();
    public DbSet<AdopcionLog> AdopcionLogs => Set<AdopcionLog>();
    #endregion

    #region Clinica
    public DbSet<Sala> Salas => Set<Sala>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<CitaRecordatorio> CitasRecordatorios => Set<CitaRecordatorio>();
    public DbSet<CitaHistorialEstado> CitasHistorialEstados => Set<CitaHistorialEstado>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketDetalle> TicketDetalles => Set<TicketDetalle>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<SolicitudCitaDigital> SolicitudesCitasDigitales => Set<SolicitudCitaDigital>();
    #endregion

    #region HistorialClinico
    public DbSet<Valoracion> Valoraciones => Set<Valoracion>();
    public DbSet<Expediente> Expedientes => Set<Expediente>();
    public DbSet<Vacunacion> Vacunaciones => Set<Vacunacion>();
    public DbSet<Desparasitacion> Desparasitaciones => Set<Desparasitacion>();
    public DbSet<Cirugia> Cirugias => Set<Cirugia>();
    public DbSet<AdjuntoMedico> AdjuntosMedicos => Set<AdjuntoMedico>();
    #endregion

    #region Inventario
    public DbSet<ItemInventario> ItemsInventario => Set<ItemInventario>();
    public DbSet<LoteInventario> LotesInventario => Set<LoteInventario>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<AlertaInventario> AlertasInventario => Set<AlertaInventario>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetallesCompras => Set<DetalleCompra>();
    public DbSet<ReporteUsoInsumos> ReportesUsoInsumos => Set<ReporteUsoInsumos>();
    public DbSet<ReporteUsoInsumoDetalle> ReportesUsoInsumosDetalles => Set<ReporteUsoInsumoDetalle>();
    public DbSet<ReporteUsoSplitLote> ReportesUsoSplitLotes => Set<ReporteUsoSplitLote>();
    #endregion

    #region Donaciones
    public DbSet<Donacion> Donaciones => Set<Donacion>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();
    #endregion

    #region Servicios
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Especialidad> Especialidades => Set<Especialidad>();
    public DbSet<EmpleadoEspecialidad> EmpleadosEspecialidades => Set<EmpleadoEspecialidad>();
    public DbSet<Horario> Horarios => Set<Horario>();
    #endregion

    #region Auditoria
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
    public DbSet<JobProgramado> JobsProgramados => Set<JobProgramado>();
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones de entidades
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdoPetsDbContext).Assembly);

        // Configuración global para decimales
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            // Default precision para montos
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // Filtros globales para Soft Delete
        modelBuilder.Entity<Mascota>().HasQueryFilter(m => m.DeletedAt == null);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is AuditableEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (AuditableEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
