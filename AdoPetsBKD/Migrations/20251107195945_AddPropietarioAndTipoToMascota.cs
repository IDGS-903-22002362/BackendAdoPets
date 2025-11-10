using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdoPetsBKD.Migrations
{
    /// <inheritdoc />
    public partial class AddPropietarioAndTipoToMascota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PropietarioId",
                table: "Mascotas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Mascotas",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Mascota_PropietarioId_Tipo",
                table: "Mascotas",
                columns: new[] { "PropietarioId", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_Mascota_Tipo",
                table: "Mascotas",
                column: "Tipo");

            migrationBuilder.AddForeignKey(
                name: "FK_Mascotas_Usuarios_PropietarioId",
                table: "Mascotas",
                column: "PropietarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mascotas_Usuarios_PropietarioId",
                table: "Mascotas");

            migrationBuilder.DropIndex(
                name: "IX_Mascota_PropietarioId_Tipo",
                table: "Mascotas");

            migrationBuilder.DropIndex(
                name: "IX_Mascota_Tipo",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "PropietarioId",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Mascotas");
        }
    }
}
