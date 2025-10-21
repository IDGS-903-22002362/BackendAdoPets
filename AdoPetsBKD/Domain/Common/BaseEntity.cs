namespace AdoPetsBKD.Domain.Common;

/// <summary>
/// Clase base para todas las entidades del dominio
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

/// <summary>
/// Entidad base con auditoría de timestamps
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

/// <summary>
/// Entidad con soporte para soft delete
/// </summary>
public abstract class SoftDeletableEntity : AuditableEntity
{
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public bool IsDeleted => DeletedAt.HasValue;

    public void Delete(Guid userId)
    {
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
    }

    public void Restore()
    {
        DeletedAt = null;
        DeletedBy = null;
    }
}
