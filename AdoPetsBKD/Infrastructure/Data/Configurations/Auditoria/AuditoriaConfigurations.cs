using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Auditoria;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Auditoria;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Entidad).HasMaxLength(255);
        builder.Property(a => a.EntidadId).HasMaxLength(50);
        builder.Property(a => a.BeforeJson).HasColumnType("nvarchar(max)");
        builder.Property(a => a.AfterJson).HasColumnType("nvarchar(max)");
        builder.Property(a => a.TraceId).HasMaxLength(100);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);

        // Índices
        builder.HasIndex(a => new { a.UsuarioId, a.CreatedAt })
            .HasDatabaseName("IX_Audit_UsuarioId_CreatedAt");

        builder.HasIndex(a => new { a.Entidad, a.EntidadId, a.CreatedAt })
            .HasDatabaseName("IX_Audit_Entidad_EntidadId_CreatedAt");

        builder.HasIndex(a => a.TraceId)
            .HasDatabaseName("IX_Audit_TraceId");

        builder.HasIndex(a => a.Accion)
            .HasDatabaseName("IX_Audit_Accion");
    }
}

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.ToTable("OutboxEvents");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.EventType).IsRequired().HasMaxLength(255);
        builder.Property(o => o.PayloadJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(o => o.ErrorMessage).HasMaxLength(2000);

        // Índices para procesamiento eficiente
        builder.HasIndex(o => new { o.ProcessedAt, o.OccurredAt })
            .HasDatabaseName("IX_Outbox_ProcessedAt_OccurredAt");

        builder.HasIndex(o => new { o.EventType, o.OccurredAt })
            .HasDatabaseName("IX_Outbox_EventType_OccurredAt");

        builder.HasIndex(o => o.Attempts)
            .HasDatabaseName("IX_Outbox_Attempts");

        // Ignorar propiedades computadas
        builder.Ignore(o => o.IsProcessed);
    }
}

public class JobProgramadoConfiguration : IEntityTypeConfiguration<JobProgramado>
{
    public void Configure(EntityTypeBuilder<JobProgramado> builder)
    {
        builder.ToTable("JobsProgramados");
        builder.HasKey(j => j.Id);

        builder.Property(j => j.RelatedEntityId).HasMaxLength(50);
        builder.Property(j => j.PayloadJson).HasColumnType("nvarchar(max)");
        builder.Property(j => j.ErrorMessage).HasMaxLength(2000);

        // Índices para procesamiento de jobs
        builder.HasIndex(j => new { j.Status, j.ScheduledFor })
            .HasDatabaseName("IX_Job_Status_ScheduledFor");

        builder.HasIndex(j => new { j.Tipo, j.ScheduledFor })
            .HasDatabaseName("IX_Job_Tipo_ScheduledFor");

        builder.HasIndex(j => j.Attempts)
            .HasDatabaseName("IX_Job_Attempts");
    }
}
