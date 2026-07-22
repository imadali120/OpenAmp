using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAmp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase2BackendApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DjelimicniPovratDoSati",
                table: "Studiji",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DjelimicniPovratPostotak",
                table: "Studiji",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PuniPovratDoSati",
                table: "Studiji",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "RadnoVrijemeDo",
                table: "Studiji",
                type: "time(0)",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "RadnoVrijemeOd",
                table: "Studiji",
                type: "time(0)",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "VremenskaZona",
                table: "Studiji",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "OtkazanaUtc",
                table: "Rezervacije",
                type: "datetime2(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazlogOtkazivanja",
                table: "Rezervacije",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundiranUtc",
                table: "Rezervacije",
                type: "datetime2(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundiraniIznos",
                table: "Rezervacije",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "StripeRefundId",
                table: "Rezervacije",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshTokeni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    KreiranUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    IsticeUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    OpozvanUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    ZamijenjenTokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    KreiranSaIpAdrese = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokeni", x => x.Id);
                    table.CheckConstraint("CK_RefreshTokeni_Datum", "[IsticeUtc] > [KreiranUtc]");
                    table.ForeignKey(
                        name: "FK_RefreshTokeni_Korisnici_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StripeWebhookDogadjaji",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Tip = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ObradjenUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripeWebhookDogadjaji", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Studiji",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DjelimicniPovratDoSati", "DjelimicniPovratPostotak", "PuniPovratDoSati", "RadnoVrijemeDo", "RadnoVrijemeOd", "VremenskaZona" },
                values: new object[] { 12, 50, 24, new TimeOnly(23, 0, 0), new TimeOnly(8, 0, 0), "Europe/Sarajevo" });

            migrationBuilder.UpdateData(
                table: "Studiji",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DjelimicniPovratDoSati", "DjelimicniPovratPostotak", "PuniPovratDoSati", "RadnoVrijemeDo", "RadnoVrijemeOd", "VremenskaZona" },
                values: new object[] { 12, 50, 24, new TimeOnly(23, 0, 0), new TimeOnly(8, 0, 0), "Europe/Sarajevo" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Studiji_PovratPostotak",
                table: "Studiji",
                sql: "[DjelimicniPovratPostotak] BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Studiji_PovratSati",
                table: "Studiji",
                sql: "[PuniPovratDoSati] >= [DjelimicniPovratDoSati] AND [DjelimicniPovratDoSati] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Studiji_RadnoVrijeme",
                table: "Studiji",
                sql: "[RadnoVrijemeDo] > [RadnoVrijemeOd]");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokeni_KorisnikId_IsticeUtc",
                table: "RefreshTokeni",
                columns: new[] { "KorisnikId", "IsticeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokeni_TokenHash",
                table: "RefreshTokeni",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokeni");

            migrationBuilder.DropTable(
                name: "StripeWebhookDogadjaji");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Studiji_PovratPostotak",
                table: "Studiji");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Studiji_PovratSati",
                table: "Studiji");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Studiji_RadnoVrijeme",
                table: "Studiji");

            migrationBuilder.DropColumn(
                name: "DjelimicniPovratDoSati",
                table: "Studiji");

            migrationBuilder.DropColumn(
                name: "DjelimicniPovratPostotak",
                table: "Studiji");

            migrationBuilder.DropColumn(
                name: "PuniPovratDoSati",
                table: "Studiji");

            migrationBuilder.DropColumn(
                name: "RadnoVrijemeDo",
                table: "Studiji");

            migrationBuilder.DropColumn(
                name: "RadnoVrijemeOd",
                table: "Studiji");

            migrationBuilder.DropColumn(
                name: "VremenskaZona",
                table: "Studiji");

            migrationBuilder.DropColumn(
                name: "OtkazanaUtc",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "RazlogOtkazivanja",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "RefundiranUtc",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "RefundiraniIznos",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "StripeRefundId",
                table: "Rezervacije");
        }
    }
}
