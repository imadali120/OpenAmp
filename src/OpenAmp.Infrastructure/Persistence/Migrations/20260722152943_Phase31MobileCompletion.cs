using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAmp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase31MobileCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "Korisnici",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OmiljeneSale",
                columns: table => new
                {
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    SalaId = table.Column<int>(type: "int", nullable: false),
                    KreiranaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OmiljeneSale", x => new { x.KorisnikId, x.SalaId });
                    table.ForeignKey(
                        name: "FK_OmiljeneSale_Korisnici_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OmiljeneSale_Sale_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Sale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostavkeKorisnika",
                columns: table => new
                {
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    PushNotifikacije = table.Column<bool>(type: "bit", nullable: false),
                    EmailNotifikacije = table.Column<bool>(type: "bit", nullable: false),
                    Jezik = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProfilJavan = table.Column<bool>(type: "bit", nullable: false),
                    AzuriraneUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostavkeKorisnika", x => x.KorisnikId);
                    table.ForeignKey(
                        name: "FK_PostavkeKorisnika_Korisnici_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_StripeCustomerId",
                table: "Korisnici",
                column: "StripeCustomerId",
                unique: true,
                filter: "[StripeCustomerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OmiljeneSale_KorisnikId_KreiranaUtc",
                table: "OmiljeneSale",
                columns: new[] { "KorisnikId", "KreiranaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OmiljeneSale_SalaId",
                table: "OmiljeneSale",
                column: "SalaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OmiljeneSale");

            migrationBuilder.DropTable(
                name: "PostavkeKorisnika");

            migrationBuilder.DropIndex(
                name: "IX_Korisnici_StripeCustomerId",
                table: "Korisnici");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "Korisnici");
        }
    }
}
