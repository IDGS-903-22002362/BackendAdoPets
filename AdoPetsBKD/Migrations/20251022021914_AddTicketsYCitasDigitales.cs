using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdoPetsBKD.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketsYCitasDigitales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "MXN"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Metodo = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    PayPalOrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PayPalCaptureId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PayPalPayerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PayPalPayerEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PayPalPayerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCancelacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Concepto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Referencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EsAnticipo = table.Column<bool>(type: "bit", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MontoRestante = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PagoPrincipalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_Pagos_PagoPrincipalId",
                        column: x => x.PagoPrincipalId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesCitasDigitales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroSolicitud = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SolicitanteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NombreMascota = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EspecieMascota = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RazaMascota = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ServicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DescripcionServicio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MotivoConsulta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaHoraSolicitada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuracionEstimadaMin = table.Column<int>(type: "int", nullable: false),
                    VeterinarioPreferidoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SalaPreferidaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRechazo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevisadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotasInternas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostoEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoAnticipo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PagoAnticipoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisponibilidadVerificada = table.Column<bool>(type: "bit", nullable: false),
                    FechaVerificacionDisponibilidad = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesCitasDigitales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesCitasDigitales_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesCitasDigitales_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesCitasDigitales_Pagos_PagoAnticipoId",
                        column: x => x.PagoAnticipoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesCitasDigitales_Salas_SalaPreferidaId",
                        column: x => x.SalaPreferidaId,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesCitasDigitales_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesCitasDigitales_Usuarios_RevisadoPorId",
                        column: x => x.RevisadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesCitasDigitales_Usuarios_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesCitasDigitales_Usuarios_VeterinarioPreferidoId",
                        column: x => x.VeterinarioPreferidoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroTicket = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CitaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VeterinarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaProcedimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NombreProcedimiento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescripcionProcedimiento = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CostoProcedimiento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoInsumos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoAdicional = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IVA = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diagnostico = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tratamiento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicacionPrescrita = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EntregadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Usuarios_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Usuarios_EntregadoPorId",
                        column: x => x.EntregadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Usuarios_VeterinarioId",
                        column: x => x.VeterinarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketDetalles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemInventarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketDetalles_ItemsInventario_ItemInventarioId",
                        column: x => x.ItemInventarioId,
                        principalTable: "ItemsInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketDetalles_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_CitaId",
                table: "Pagos",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_NumeroPago",
                table: "Pagos",
                column: "NumeroPago",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_PagoPrincipalId",
                table: "Pagos",
                column: "PagoPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_PayPalCaptureId",
                table: "Pagos",
                column: "PayPalCaptureId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_PayPalOrderId",
                table: "Pagos",
                column: "PayPalOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_UsuarioId",
                table: "Pagos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_CitaId",
                table: "SolicitudesCitasDigitales",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_Estado",
                table: "SolicitudesCitasDigitales",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_FechaHoraSolicitada",
                table: "SolicitudesCitasDigitales",
                column: "FechaHoraSolicitada");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_FechaSolicitud",
                table: "SolicitudesCitasDigitales",
                column: "FechaSolicitud");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_MascotaId",
                table: "SolicitudesCitasDigitales",
                column: "MascotaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_NumeroSolicitud",
                table: "SolicitudesCitasDigitales",
                column: "NumeroSolicitud",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_PagoAnticipoId",
                table: "SolicitudesCitasDigitales",
                column: "PagoAnticipoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_RevisadoPorId",
                table: "SolicitudesCitasDigitales",
                column: "RevisadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_SalaPreferidaId",
                table: "SolicitudesCitasDigitales",
                column: "SalaPreferidaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_ServicioId",
                table: "SolicitudesCitasDigitales",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_SolicitanteId",
                table: "SolicitudesCitasDigitales",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCitasDigitales_VeterinarioPreferidoId",
                table: "SolicitudesCitasDigitales",
                column: "VeterinarioPreferidoId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketDetalles_ItemInventarioId",
                table: "TicketDetalles",
                column: "ItemInventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketDetalles_TicketId",
                table: "TicketDetalles",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CitaId",
                table: "Tickets",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ClienteId",
                table: "Tickets",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EntregadoPorId",
                table: "Tickets",
                column: "EntregadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_MascotaId",
                table: "Tickets",
                column: "MascotaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_NumeroTicket",
                table: "Tickets",
                column: "NumeroTicket",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PagoId",
                table: "Tickets",
                column: "PagoId",
                unique: true,
                filter: "[PagoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_VeterinarioId",
                table: "Tickets",
                column: "VeterinarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitudesCitasDigitales");

            migrationBuilder.DropTable(
                name: "TicketDetalles");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Pagos");
        }
    }
}
