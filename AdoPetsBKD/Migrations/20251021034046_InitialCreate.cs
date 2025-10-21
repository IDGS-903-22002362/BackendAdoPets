using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AdoPetsBKD.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Entidad = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EntidadId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Accion = table.Column<int>(type: "int", nullable: false),
                    BeforeJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TraceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Especialidades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemsInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    MinQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsInventario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobsProgramados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    RelatedEntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduledFor = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobsProgramados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mascotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Especie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Raza = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sexo = table.Column<int>(type: "int", nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    Personalidad = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EstadoSalud = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequisitoAdopcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Origen = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mascotas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    RFC = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    Contacto = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Salas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CapacidadMaxima = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DuracionMinDefault = table.Column<int>(type: "int", nullable: false),
                    PrecioSugerido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Categoria = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApellidoPaterno = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApellidoMaterno = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    UltimoAccesoAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AceptoPoliticasVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AceptoPoliticasAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Retries = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LastRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertasInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertasInventario_ItemsInventario_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ItemsInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LotesInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Lote = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QtyDisponible = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QtyInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotesInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotesInventario_ItemsInventario_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ItemsInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdjuntosMedicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntryType = table.Column<int>(type: "int", nullable: false),
                    EntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdjuntosMedicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdjuntosMedicos_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cirugias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VeterinarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Anesthesia = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DuracionMin = table.Column<int>(type: "int", nullable: true),
                    Complications = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Medicacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CuidadosPostoperatorios = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cirugias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cirugias_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Desparasitaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Product = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Dose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextDueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VeterinarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Desparasitaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Desparasitaciones_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MascotasFotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MascotasFotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MascotasFotos_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vacunaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VaccineName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Dose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Lot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextDueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VeterinarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReaccionAdversa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacunaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vacunaciones_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroFactura = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaCompra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecibidoPor = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compras_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PropietarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VeterinarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuracionMin = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Citas_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Citas_Salas_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Citas_Usuarios_PropietarioId",
                        column: x => x.PropietarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Citas_Usuarios_VeterinarioId",
                        column: x => x.VeterinarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsentimientosPoliticas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentimientosPoliticas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentimientosPoliticas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dispositivos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Plataforma = table.Column<int>(type: "int", nullable: false),
                    AppVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispositivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dispositivos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Donaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "MXN"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaypalOrderId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PaypalCaptureId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PayerEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PayerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PaypalPayerId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Source = table.Column<int>(type: "int", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Anonima = table.Column<bool>(type: "bit", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Donaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Disponibilidad = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmailLaboral = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TelefonoLaboral = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Sueldo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FechaContratacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaBaja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Empleados_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreferenciasNotificaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Canal = table.Column<int>(type: "int", nullable: false),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferenciasNotificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreferenciasNotificaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesAdopcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Vivienda = table.Column<int>(type: "int", nullable: false),
                    NumNinios = table.Column<int>(type: "int", nullable: false),
                    OtrasMascotas = table.Column<bool>(type: "bit", nullable: false),
                    HorasDisponibilidad = table.Column<int>(type: "int", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IngresosMensuales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MotivoAdopcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevisadoPor = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesAdopcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesAdopcion_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesAdopcion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosRoles",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AsignadoAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AsignadoPor = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosRoles", x => new { x.UsuarioId, x.RolId });
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesCompras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Lote = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesCompras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesCompras_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesCompras_ItemsInventario_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ItemsInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CitasHistorialEstados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    ChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitasHistorialEstados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitasHistorialEstados_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CitasRecordatorios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitasRecordatorios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitasRecordatorios_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Expedientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VeterinarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Anamnesis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Diagnostico = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Tratamiento = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Pronostico = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expedientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expedientes_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Expedientes_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PerformedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelatedAppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Citas_RelatedAppointmentId",
                        column: x => x.RelatedAppointmentId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_ItemsInventario_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ItemsInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_LotesInventario_BatchId",
                        column: x => x.BatchId,
                        principalTable: "LotesInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportesUsoInsumos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ClientUsageId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RevertedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevertedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RevertReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesUsoInsumos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportesUsoInsumos_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Valoraciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Peso = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Temperatura = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FrecuenciaCardiaca = table.Column<int>(type: "int", nullable: true),
                    FrecuenciaRespiratoria = table.Column<int>(type: "int", nullable: true),
                    CondicionCorporal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TakenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TakenBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Valoraciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Valoraciones_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Valoraciones_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpleadosEspecialidades",
                columns: table => new
                {
                    EmpleadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EspecialidadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObtainedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Certificacion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpleadosEspecialidades", x => new { x.EmpleadoId, x.EspecialidadId });
                    table.ForeignKey(
                        name: "FK_EmpleadosEspecialidades_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpleadosEspecialidades_Especialidades_EspecialidadId",
                        column: x => x.EspecialidadId,
                        principalTable: "Especialidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Horarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpleadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RangoInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RangoFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HoraEntrada = table.Column<TimeSpan>(type: "time", nullable: true),
                    HoraSalida = table.Column<TimeSpan>(type: "time", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Horarios_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdopcionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromEstado = table.Column<int>(type: "int", nullable: false),
                    ToEstado = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdopcionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdopcionLogs_SolicitudesAdopcion_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "SolicitudesAdopcion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesAdopcionAdjuntos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesAdopcionAdjuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesAdopcionAdjuntos_SolicitudesAdopcion_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "SolicitudesAdopcion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdjuntoMedicoExpediente",
                columns: table => new
                {
                    AdjuntosId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpedienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdjuntoMedicoExpediente", x => new { x.AdjuntosId, x.ExpedienteId });
                    table.ForeignKey(
                        name: "FK_AdjuntoMedicoExpediente_AdjuntosMedicos_AdjuntosId",
                        column: x => x.AdjuntosId,
                        principalTable: "AdjuntosMedicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdjuntoMedicoExpediente_Expedientes_ExpedienteId",
                        column: x => x.ExpedienteId,
                        principalTable: "Expedientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportesUsoInsumosDetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReporteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QtySolicitada = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QtyAplicada = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesUsoInsumosDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportesUsoInsumosDetalles_ItemsInventario_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ItemsInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportesUsoInsumosDetalles_ReportesUsoInsumos_ReporteId",
                        column: x => x.ReporteId,
                        principalTable: "ReportesUsoInsumos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportesUsoSplitLotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReporteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DetalleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QtyConsumida = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesUsoSplitLotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportesUsoSplitLotes_LotesInventario_BatchId",
                        column: x => x.BatchId,
                        principalTable: "LotesInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportesUsoSplitLotes_ReportesUsoInsumosDetalles_DetalleId",
                        column: x => x.DetalleId,
                        principalTable: "ReportesUsoInsumosDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportesUsoSplitLotes_ReportesUsoInsumos_ReporteId",
                        column: x => x.ReporteId,
                        principalTable: "ReportesUsoInsumos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Administrador del sistema", "Admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Veterinario", "Veterinario" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Recepcionista", "Recepcionista" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Asistente del refugio", "Asistente" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Usuario adoptante", "Adoptante" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdjuntoMedicoExpediente_ExpedienteId",
                table: "AdjuntoMedicoExpediente",
                column: "ExpedienteId");

            migrationBuilder.CreateIndex(
                name: "IX_AdjMed_EntryType_EntryId",
                table: "AdjuntosMedicos",
                columns: new[] { "EntryType", "EntryId" });

            migrationBuilder.CreateIndex(
                name: "IX_AdjMed_MascotaId_UploadedAt",
                table: "AdjuntosMedicos",
                columns: new[] { "MascotaId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AdopLog_SolicitudId_ChangedAt",
                table: "AdopcionLogs",
                columns: new[] { "SolicitudId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Alert_ItemId_Status_Tipo",
                table: "AlertasInventario",
                columns: new[] { "ItemId", "Status", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_Alert_Status_CreatedAt",
                table: "AlertasInventario",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Audit_Accion",
                table: "AuditLogs",
                column: "Accion");

            migrationBuilder.CreateIndex(
                name: "IX_Audit_Entidad_EntidadId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "Entidad", "EntidadId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Audit_TraceId",
                table: "AuditLogs",
                column: "TraceId");

            migrationBuilder.CreateIndex(
                name: "IX_Audit_UsuarioId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "UsuarioId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Cirugia_MascotaId_PerformedAt",
                table: "Cirugias",
                columns: new[] { "MascotaId", "PerformedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Cita_MascotaId_StartAt",
                table: "Citas",
                columns: new[] { "MascotaId", "StartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Cita_Sala_Start_Status",
                table: "Citas",
                columns: new[] { "SalaId", "StartAt", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Cita_Status",
                table: "Citas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Cita_Vet_Start_Status",
                table: "Citas",
                columns: new[] { "VeterinarioId", "StartAt", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_PropietarioId",
                table: "Citas",
                column: "PropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CitaHistorial_CitaId_ChangedAt",
                table: "CitasHistorialEstados",
                columns: new[] { "CitaId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_CitaRecordatorio_CitaId_Tipo",
                table: "CitasRecordatorios",
                columns: new[] { "CitaId", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Compra_ProveedorId_Fecha",
                table: "Compras",
                columns: new[] { "ProveedorId", "FechaCompra" });

            migrationBuilder.CreateIndex(
                name: "IX_Compra_Status",
                table: "Compras",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Consentimiento_UsuarioId_Version",
                table: "ConsentimientosPoliticas",
                columns: new[] { "UsuarioId", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_Desp_MascotaId_AppliedAt",
                table: "Desparasitaciones",
                columns: new[] { "MascotaId", "AppliedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleCompra_CompraId",
                table: "DetallesCompras",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCompras_ItemId",
                table: "DetallesCompras",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Dispositivo_UsuarioId",
                table: "Dispositivos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UX_Dispositivo_Token",
                table: "Dispositivos",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donacion_PaypalOrderId",
                table: "Donaciones",
                column: "PaypalOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Donacion_Status_CreatedAt",
                table: "Donaciones",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Donaciones_UsuarioId",
                table: "Donaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UX_Donacion_PaypalCaptureId",
                table: "Donaciones",
                column: "PaypalCaptureId",
                unique: true,
                filter: "[PaypalCaptureId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Empleado_Activo",
                table: "Empleados",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "UX_Empleado_UsuarioId",
                table: "Empleados",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadosEspecialidades_EspecialidadId",
                table: "EmpleadosEspecialidades",
                column: "EspecialidadId");

            migrationBuilder.CreateIndex(
                name: "UX_Especialidad_Descripcion",
                table: "Especialidades",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expediente_MascotaId_Fecha",
                table: "Expedientes",
                columns: new[] { "MascotaId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_CitaId",
                table: "Expedientes",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Horario_EmpleadoId_Fecha",
                table: "Horarios",
                columns: new[] { "EmpleadoId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Horario_EmpleadoId_Rango",
                table: "Horarios",
                columns: new[] { "EmpleadoId", "RangoInicio", "RangoFin" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventario_Activo",
                table: "ItemsInventario",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventario_Categoria",
                table: "ItemsInventario",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "UX_ItemInventario_Nombre",
                table: "ItemsInventario",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Job_Attempts",
                table: "JobsProgramados",
                column: "Attempts");

            migrationBuilder.CreateIndex(
                name: "IX_Job_Status_ScheduledFor",
                table: "JobsProgramados",
                columns: new[] { "Status", "ScheduledFor" });

            migrationBuilder.CreateIndex(
                name: "IX_Job_Tipo_ScheduledFor",
                table: "JobsProgramados",
                columns: new[] { "Tipo", "ScheduledFor" });

            migrationBuilder.CreateIndex(
                name: "IX_Lote_ExpDate",
                table: "LotesInventario",
                column: "ExpDate");

            migrationBuilder.CreateIndex(
                name: "IX_Lote_ItemId_ExpDate",
                table: "LotesInventario",
                columns: new[] { "ItemId", "ExpDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Lote_ItemId_QtyDisponible",
                table: "LotesInventario",
                columns: new[] { "ItemId", "QtyDisponible" });

            migrationBuilder.CreateIndex(
                name: "IX_Mascota_DeletedAt",
                table: "Mascotas",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Mascota_Especie_Estatus",
                table: "Mascotas",
                columns: new[] { "Especie", "Estatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Mascota_Estatus",
                table: "Mascotas",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_MascotaFoto_MascotaId_Orden",
                table: "MascotasFotos",
                columns: new[] { "MascotaId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_Mov_AppointmentId",
                table: "MovimientosInventario",
                column: "RelatedAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Mov_BatchId_CreatedAt",
                table: "MovimientosInventario",
                columns: new[] { "BatchId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Mov_ItemId_CreatedAt",
                table: "MovimientosInventario",
                columns: new[] { "ItemId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notif_Tipo_Fecha",
                table: "Notificaciones",
                columns: new[] { "Tipo", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Notif_UsuarioId_Fecha",
                table: "Notificaciones",
                columns: new[] { "UsuarioId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_Attempts",
                table: "OutboxEvents",
                column: "Attempts");

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_EventType_OccurredAt",
                table: "OutboxEvents",
                columns: new[] { "EventType", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_ProcessedAt_OccurredAt",
                table: "OutboxEvents",
                columns: new[] { "ProcessedAt", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "UX_PreferenciaNotif_Usuario_Canal_Categoria",
                table: "PreferenciasNotificaciones",
                columns: new[] { "UsuarioId", "Canal", "Categoria" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proveedor_Email",
                table: "Proveedores",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedor_Estatus",
                table: "Proveedores",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_Report_Status_SubmittedAt",
                table: "ReportesUsoInsumos",
                columns: new[] { "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_Report_Appointment",
                table: "ReportesUsoInsumos",
                column: "CitaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Report_ClientUsageId",
                table: "ReportesUsoInsumos",
                column: "ClientUsageId",
                unique: true,
                filter: "[ClientUsageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReporteDetalle_ItemId",
                table: "ReportesUsoInsumosDetalles",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReporteDetalle_ReporteId",
                table: "ReportesUsoInsumosDetalles",
                column: "ReporteId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesUsoSplitLotes_DetalleId",
                table: "ReportesUsoSplitLotes",
                column: "DetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_Split_BatchId",
                table: "ReportesUsoSplitLotes",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Split_ReporteId_DetalleId",
                table: "ReportesUsoSplitLotes",
                columns: new[] { "ReporteId", "DetalleId" });

            migrationBuilder.CreateIndex(
                name: "UX_Rol_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Sala_Nombre",
                table: "Salas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Servicio_Activo",
                table: "Servicios",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "UX_Servicio_Descripcion",
                table: "Servicios",
                column: "Descripcion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Adopcion_Estado",
                table: "SolicitudesAdopcion",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Adopcion_MascotaId_Estado",
                table: "SolicitudesAdopcion",
                columns: new[] { "MascotaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Adopcion_UsuarioId_Fecha",
                table: "SolicitudesAdopcion",
                columns: new[] { "UsuarioId", "FechaSolicitud" });

            migrationBuilder.CreateIndex(
                name: "IX_Adjunto_SolicitudId_UploadedAt",
                table: "SolicitudesAdopcionAdjuntos",
                columns: new[] { "SolicitudId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Estatus",
                table: "Usuarios",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "UX_Usuario_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRol_UsuarioId",
                table: "UsuariosRoles",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosRoles_RolId",
                table: "UsuariosRoles",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacuna_MascotaId_AppliedAt",
                table: "Vacunaciones",
                columns: new[] { "MascotaId", "AppliedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Valoracion_MascotaId_TakenAt",
                table: "Valoraciones",
                columns: new[] { "MascotaId", "TakenAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Valoraciones_CitaId",
                table: "Valoraciones",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Webhook_Provider_Status_ReceivedAt",
                table: "WebhookEvents",
                columns: new[] { "Provider", "Status", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_Webhook_EventId",
                table: "WebhookEvents",
                column: "EventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdjuntoMedicoExpediente");

            migrationBuilder.DropTable(
                name: "AdopcionLogs");

            migrationBuilder.DropTable(
                name: "AlertasInventario");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Cirugias");

            migrationBuilder.DropTable(
                name: "CitasHistorialEstados");

            migrationBuilder.DropTable(
                name: "CitasRecordatorios");

            migrationBuilder.DropTable(
                name: "ConsentimientosPoliticas");

            migrationBuilder.DropTable(
                name: "Desparasitaciones");

            migrationBuilder.DropTable(
                name: "DetallesCompras");

            migrationBuilder.DropTable(
                name: "Dispositivos");

            migrationBuilder.DropTable(
                name: "Donaciones");

            migrationBuilder.DropTable(
                name: "EmpleadosEspecialidades");

            migrationBuilder.DropTable(
                name: "Horarios");

            migrationBuilder.DropTable(
                name: "JobsProgramados");

            migrationBuilder.DropTable(
                name: "MascotasFotos");

            migrationBuilder.DropTable(
                name: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "OutboxEvents");

            migrationBuilder.DropTable(
                name: "PreferenciasNotificaciones");

            migrationBuilder.DropTable(
                name: "ReportesUsoSplitLotes");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "SolicitudesAdopcionAdjuntos");

            migrationBuilder.DropTable(
                name: "UsuariosRoles");

            migrationBuilder.DropTable(
                name: "Vacunaciones");

            migrationBuilder.DropTable(
                name: "Valoraciones");

            migrationBuilder.DropTable(
                name: "WebhookEvents");

            migrationBuilder.DropTable(
                name: "AdjuntosMedicos");

            migrationBuilder.DropTable(
                name: "Expedientes");

            migrationBuilder.DropTable(
                name: "Compras");

            migrationBuilder.DropTable(
                name: "Especialidades");

            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "LotesInventario");

            migrationBuilder.DropTable(
                name: "ReportesUsoInsumosDetalles");

            migrationBuilder.DropTable(
                name: "SolicitudesAdopcion");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "ItemsInventario");

            migrationBuilder.DropTable(
                name: "ReportesUsoInsumos");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Mascotas");

            migrationBuilder.DropTable(
                name: "Salas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
