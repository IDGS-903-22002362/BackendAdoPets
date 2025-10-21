using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Servicios;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Servicios;

public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
{
    public void Configure(EntityTypeBuilder<Servicio> builder)
    {
        builder.ToTable("Servicios");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Descripcion).IsRequired().HasMaxLength(255);
        builder.Property(s => s.PrecioSugerido).HasPrecision(18, 2);
        builder.Property(s => s.Notas).HasMaxLength(1000);

        builder.HasIndex(s => s.Descripcion)
            .IsUnique()
            .HasDatabaseName("UX_Servicio_Descripcion");

        builder.HasIndex(s => s.Activo)
            .HasDatabaseName("IX_Servicio_Activo");
    }
}

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("Empleados");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Cedula).HasMaxLength(50);
        builder.Property(e => e.Disponibilidad).HasMaxLength(1000);
        builder.Property(e => e.EmailLaboral).HasMaxLength(255);
        builder.Property(e => e.TelefonoLaboral).HasMaxLength(20);
        builder.Property(e => e.Sueldo).HasPrecision(18, 2);

        builder.HasIndex(e => e.UsuarioId)
            .IsUnique()
            .HasDatabaseName("UX_Empleado_UsuarioId");

        builder.HasIndex(e => e.Activo)
            .HasDatabaseName("IX_Empleado_Activo");

        // Relaciones
        builder.HasOne(e => e.Usuario)
            .WithMany()
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Especialidades)
            .WithOne(es => es.Empleado)
            .HasForeignKey(es => es.EmpleadoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Horarios)
            .WithOne(h => h.Empleado)
            .HasForeignKey(h => h.EmpleadoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EspecialidadConfiguration : IEntityTypeConfiguration<Especialidad>
{
    public void Configure(EntityTypeBuilder<Especialidad> builder)
    {
        builder.ToTable("Especialidades");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Descripcion).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Codigo).HasMaxLength(50);

        builder.HasIndex(e => e.Descripcion)
            .IsUnique()
            .HasDatabaseName("UX_Especialidad_Descripcion");
    }
}

public class EmpleadoEspecialidadConfiguration : IEntityTypeConfiguration<EmpleadoEspecialidad>
{
    public void Configure(EntityTypeBuilder<EmpleadoEspecialidad> builder)
    {
        builder.ToTable("EmpleadosEspecialidades");
        
        // Clave compuesta
        builder.HasKey(ee => new { ee.EmpleadoId, ee.EspecialidadId });

        builder.Property(ee => ee.Certificacion).HasMaxLength(255);

        // Relaciones
        builder.HasOne(ee => ee.Empleado)
            .WithMany(e => e.Especialidades)
            .HasForeignKey(ee => ee.EmpleadoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ee => ee.Especialidad)
            .WithMany(e => e.EmpleadoEspecialidades)
            .HasForeignKey(ee => ee.EspecialidadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class HorarioConfiguration : IEntityTypeConfiguration<Horario>
{
    public void Configure(EntityTypeBuilder<Horario> builder)
    {
        builder.ToTable("Horarios");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Notas).HasMaxLength(1000);

        builder.HasIndex(h => new { h.EmpleadoId, h.Fecha })
            .HasDatabaseName("IX_Horario_EmpleadoId_Fecha");

        builder.HasIndex(h => new { h.EmpleadoId, h.RangoInicio, h.RangoFin })
            .HasDatabaseName("IX_Horario_EmpleadoId_Rango");

        // Relaciones
        builder.HasOne(h => h.Empleado)
            .WithMany(e => e.Horarios)
            .HasForeignKey(h => h.EmpleadoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
