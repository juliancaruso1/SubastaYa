using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SubastaYa.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditoriaLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Entidad = table.Column<string>(type: "TEXT", nullable: false),
                    EntidadId = table.Column<int>(type: "INTEGER", nullable: false),
                    Accion = table.Column<string>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    DetalleJson = table.Column<string>(type: "TEXT", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    UrlIcono = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Billeteras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    SaldoTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    SaldoRetenido = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billeteras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Billeteras_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subastas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VendedorId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    UrlImagen = table.Column<string>(type: "TEXT", nullable: false),
                    PrecioBase = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    IncrementoMinimo = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subastas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subastas_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subastas_Usuarios_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransaccionesLedger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BilleteraId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Monto = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubastaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaccionesLedger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransaccionesLedger_Billeteras_BilleteraId",
                        column: x => x.BilleteraId,
                        principalTable: "Billeteras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pujas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubastaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CompradorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Monto = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FechaPuja = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pujas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pujas_Subastas_SubastaId",
                        column: x => x.SubastaId,
                        principalTable: "Subastas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pujas_Usuarios_CompradorId",
                        column: x => x.CompradorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Nombre", "UrlIcono" },
                values: new object[,]
                {
                    { 1, "Tecnología", null },
                    { 2, "Coleccionables", null },
                    { 3, "Indumentaria", null },
                    { 4, "Vehículos", null }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "FechaRegistro", "Nombre", "PasswordHash" },
                values: new object[,]
                {
                    { 1, "vendedor@test.com", new DateTime(2026, 9, 6, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), "Vendedor Demo", "demo-hash" },
                    { 2, "comprador1@test.com", new DateTime(2026, 9, 6, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), "Comprador Uno", "demo-hash" },
                    { 3, "comprador2@test.com", new DateTime(2026, 9, 6, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), "Comprador Dos", "demo-hash" },
                    { 4, "sinfondos@test.com", new DateTime(2026, 9, 6, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), "Sin Fondos", "demo-hash" }
                });

            migrationBuilder.InsertData(
                table: "Billeteras",
                columns: new[] { "Id", "SaldoRetenido", "SaldoTotal", "UsuarioId", "Version" },
                values: new object[,]
                {
                    { 1, 0m, 0m, 1, 0 },
                    { 2, 45000m, 150000m, 2, 1 },
                    { 3, 0m, 200000m, 3, 0 },
                    { 4, 0m, 500m, 4, 0 }
                });

            migrationBuilder.InsertData(
                table: "Subastas",
                columns: new[] { "Id", "CategoriaId", "Descripcion", "Estado", "FechaFin", "FechaInicio", "IncrementoMinimo", "PrecioBase", "Titulo", "UrlImagen", "VendedorId", "Version" },
                values: new object[,]
                {
                    { 1, 1, "Activa estándar, con 2 pujas previas.", 1, new DateTime(2026, 9, 6, 22, 39, 44, 158, DateTimeKind.Utc).AddTicks(5547), new DateTime(2026, 9, 6, 21, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), 1000m, 30000m, "Notebook Gamer RTX", "https://picsum.photos/seed/notebook/400/300", 1, 2 },
                    { 2, 2, "Activa crítica: cierra en menos de 2 minutos, para probar anti-sniping.", 1, new DateTime(2026, 9, 6, 22, 19, 44, 158, DateTimeKind.Utc).AddTicks(5547), new DateTime(2026, 9, 6, 20, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), 500m, 5000m, "Figura de colección edición limitada", "https://picsum.photos/seed/figura/400/300", 1, 0 },
                    { 3, 4, "Próxima: inicio programado a +24hs, pujas bloqueadas.", 0, new DateTime(2026, 9, 8, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), new DateTime(2026, 9, 7, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), 10000m, 800000m, "Moto 150cc", "https://picsum.photos/seed/moto/400/300", 1, 0 },
                    { 4, 3, "Vencida con ganador: para probar cierre y liquidación del Worker.", 1, new DateTime(2026, 9, 6, 22, 4, 44, 158, DateTimeKind.Utc).AddTicks(5547), new DateTime(2026, 9, 4, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), 500m, 8000m, "Campera de cuero vintage", "https://picsum.photos/seed/campera/400/300", 1, 1 },
                    { 5, 1, "Vencida desierta: sin pujas, para probar pase a DESIERTA.", 1, new DateTime(2026, 9, 6, 22, 9, 44, 158, DateTimeKind.Utc).AddTicks(5547), new DateTime(2026, 9, 4, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), 200m, 3000m, "Mouse gamer inalámbrico", "https://picsum.photos/seed/mouse/400/300", 1, 0 }
                });

            migrationBuilder.InsertData(
                table: "Pujas",
                columns: new[] { "Id", "CompradorId", "FechaPuja", "Monto", "SubastaId" },
                values: new object[,]
                {
                    { 1, 3, new DateTime(2026, 9, 6, 21, 34, 44, 158, DateTimeKind.Utc).AddTicks(5547), 32000m, 1 },
                    { 2, 2, new DateTime(2026, 9, 6, 21, 54, 44, 158, DateTimeKind.Utc).AddTicks(5547), 45000m, 1 },
                    { 3, 3, new DateTime(2026, 9, 5, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), 9500m, 4 }
                });

            migrationBuilder.InsertData(
                table: "TransaccionesLedger",
                columns: new[] { "Id", "BilleteraId", "Fecha", "Monto", "SubastaId", "Tipo" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2026, 9, 3, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), 150000m, null, 0 },
                    { 2, 3, new DateTime(2026, 9, 3, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), 200000m, null, 0 },
                    { 3, 4, new DateTime(2026, 9, 3, 22, 14, 44, 158, DateTimeKind.Utc).AddTicks(5547), 500m, null, 0 },
                    { 4, 2, new DateTime(2026, 9, 6, 21, 54, 44, 158, DateTimeKind.Utc).AddTicks(5547), 45000m, 1, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Billeteras_UsuarioId",
                table: "Billeteras",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pujas_CompradorId",
                table: "Pujas",
                column: "CompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pujas_SubastaId",
                table: "Pujas",
                column: "SubastaId");

            migrationBuilder.CreateIndex(
                name: "IX_Subastas_CategoriaId",
                table: "Subastas",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Subastas_VendedorId",
                table: "Subastas",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesLedger_BilleteraId",
                table: "TransaccionesLedger",
                column: "BilleteraId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaLogs");

            migrationBuilder.DropTable(
                name: "Pujas");

            migrationBuilder.DropTable(
                name: "TransaccionesLedger");

            migrationBuilder.DropTable(
                name: "Subastas");

            migrationBuilder.DropTable(
                name: "Billeteras");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
