using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAmp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase4DesktopOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stanje",
                table: "Oprema",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.CreateTable(
                name: "ServisiOpreme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OpremaId = table.Column<int>(type: "int", nullable: false),
                    PrijavljenUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ZavrsenUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpisKvara = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IzvrseniRadovi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Trosak = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PrijavioKorisnikId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServisiOpreme", x => x.Id);
                    table.CheckConstraint("CK_ServisiOpreme_Trosak", "[Trosak] >= 0");
                    table.ForeignKey(
                        name: "FK_ServisiOpreme_Korisnici_PrijavioKorisnikId",
                        column: x => x.PrijavioKorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServisiOpreme_Oprema_OpremaId",
                        column: x => x.OpremaId,
                        principalTable: "Oprema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Oprema",
                keyColumn: "Id",
                keyValue: 1,
                column: "Stanje",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Oprema",
                keyColumn: "Id",
                keyValue: 2,
                column: "Stanje",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Oprema",
                keyColumn: "Id",
                keyValue: 3,
                column: "Stanje",
                value: 4);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Oprema_Stanje",
                table: "Oprema",
                sql: "[Stanje] BETWEEN 1 AND 5");

            migrationBuilder.CreateIndex(
                name: "IX_ServisiOpreme_OpremaId_PrijavljenUtc",
                table: "ServisiOpreme",
                columns: new[] { "OpremaId", "PrijavljenUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ServisiOpreme_PrijavioKorisnikId",
                table: "ServisiOpreme",
                column: "PrijavioKorisnikId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServisiOpreme");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Oprema_Stanje",
                table: "Oprema");

            migrationBuilder.DropColumn(
                name: "Stanje",
                table: "Oprema");
        }
    }
}
