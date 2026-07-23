using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAmp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UsernamesAndDatabaseImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FotografijaId",
                table: "Studiji",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotografijaUrl",
                table: "Studiji",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedijskaDatotekaId",
                table: "SlikeSala",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "PozivniceBenda",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfilnaSlikaId",
                table: "Korisnici",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Korisnici",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FotografijaId",
                table: "Bendovi",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MedijskeDatoteke",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NazivDatoteke = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sadrzaj = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Velicina = table.Column<long>(type: "bigint", nullable: false),
                    KreiranaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    KreiraoKorisnikId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedijskeDatoteke", x => x.Id);
                    table.CheckConstraint("CK_MedijskeDatoteke_Velicina", "[Velicina] > 0 AND [Velicina] <= 5242880");
                    table.ForeignKey(
                        name: "FK_MedijskeDatoteke_Korisnici_KreiraoKorisnikId",
                        column: x => x.KreiraoKorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                UPDATE [Korisnici]
                SET [Username] = CONCAT('user', [Id])
                WHERE [Username] IS NULL;

                UPDATE p
                SET p.[Username] = k.[Username]
                FROM [PozivniceBenda] p
                INNER JOIN [Korisnici] k ON k.[Id] = p.[PozvaniKorisnikId];

                DELETE FROM [PozivniceBenda]
                WHERE [PozvaniKorisnikId] IS NULL OR [Username] IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Korisnici",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "PozivniceBenda",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PozvaniKorisnikId",
                table: "PozivniceBenda",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_PozivniceBenda_BendId_Email_StatusPozivniceId",
                table: "PozivniceBenda");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "PozivniceBenda");

            migrationBuilder.UpdateData(
                table: "SlikeSala",
                keyColumn: "Id",
                keyValue: 1,
                column: "MedijskaDatotekaId",
                value: null);

            migrationBuilder.UpdateData(
                table: "SlikeSala",
                keyColumn: "Id",
                keyValue: 2,
                column: "MedijskaDatotekaId",
                value: null);

            migrationBuilder.UpdateData(
                table: "SlikeSala",
                keyColumn: "Id",
                keyValue: 3,
                column: "MedijskaDatotekaId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Studiji",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FotografijaId", "FotografijaUrl" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Studiji",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "FotografijaId", "FotografijaUrl" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Studiji_FotografijaId",
                table: "Studiji",
                column: "FotografijaId");

            migrationBuilder.CreateIndex(
                name: "IX_SlikeSala_MedijskaDatotekaId",
                table: "SlikeSala",
                column: "MedijskaDatotekaId");

            migrationBuilder.CreateIndex(
                name: "IX_PozivniceBenda_BendId_Username_StatusPozivniceId",
                table: "PozivniceBenda",
                columns: new[] { "BendId", "Username", "StatusPozivniceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_ProfilnaSlikaId",
                table: "Korisnici",
                column: "ProfilnaSlikaId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_Username",
                table: "Korisnici",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bendovi_FotografijaId",
                table: "Bendovi",
                column: "FotografijaId");

            migrationBuilder.CreateIndex(
                name: "IX_MedijskeDatoteke_KreiraoKorisnikId_KreiranaUtc",
                table: "MedijskeDatoteke",
                columns: new[] { "KreiraoKorisnikId", "KreiranaUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bendovi_MedijskeDatoteke_FotografijaId",
                table: "Bendovi",
                column: "FotografijaId",
                principalTable: "MedijskeDatoteke",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnici_MedijskeDatoteke_ProfilnaSlikaId",
                table: "Korisnici",
                column: "ProfilnaSlikaId",
                principalTable: "MedijskeDatoteke",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SlikeSala_MedijskeDatoteke_MedijskaDatotekaId",
                table: "SlikeSala",
                column: "MedijskaDatotekaId",
                principalTable: "MedijskeDatoteke",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Studiji_MedijskeDatoteke_FotografijaId",
                table: "Studiji",
                column: "FotografijaId",
                principalTable: "MedijskeDatoteke",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bendovi_MedijskeDatoteke_FotografijaId",
                table: "Bendovi");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnici_MedijskeDatoteke_ProfilnaSlikaId",
                table: "Korisnici");

            migrationBuilder.DropForeignKey(
                name: "FK_SlikeSala_MedijskeDatoteke_MedijskaDatotekaId",
                table: "SlikeSala");

            migrationBuilder.DropForeignKey(
                name: "FK_Studiji_MedijskeDatoteke_FotografijaId",
                table: "Studiji");

            migrationBuilder.DropTable(
                name: "MedijskeDatoteke");

            migrationBuilder.DropIndex(
                name: "IX_Studiji_FotografijaId",
                table: "Studiji");

            migrationBuilder.DropIndex(
                name: "IX_SlikeSala_MedijskaDatotekaId",
                table: "SlikeSala");

            migrationBuilder.DropIndex(
                name: "IX_PozivniceBenda_BendId_Username_StatusPozivniceId",
                table: "PozivniceBenda");

            migrationBuilder.DropIndex(
                name: "IX_Korisnici_ProfilnaSlikaId",
                table: "Korisnici");

            migrationBuilder.DropIndex(
                name: "IX_Korisnici_Username",
                table: "Korisnici");

            migrationBuilder.DropIndex(
                name: "IX_Bendovi_FotografijaId",
                table: "Bendovi");

            migrationBuilder.DropColumn(
                name: "FotografijaId",
                table: "Studiji");

            migrationBuilder.DropColumn(
                name: "FotografijaUrl",
                table: "Studiji");

            migrationBuilder.DropColumn(
                name: "MedijskaDatotekaId",
                table: "SlikeSala");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "PozivniceBenda");

            migrationBuilder.DropColumn(
                name: "ProfilnaSlikaId",
                table: "Korisnici");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Korisnici");

            migrationBuilder.DropColumn(
                name: "FotografijaId",
                table: "Bendovi");

            migrationBuilder.AlterColumn<int>(
                name: "PozvaniKorisnikId",
                table: "PozivniceBenda",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "PozivniceBenda",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE p
                SET p.[Email] = k.[Email]
                FROM [PozivniceBenda] p
                INNER JOIN [Korisnici] k ON k.[Id] = p.[PozvaniKorisnikId];
                """);

            migrationBuilder.CreateIndex(
                name: "IX_PozivniceBenda_BendId_Email_StatusPozivniceId",
                table: "PozivniceBenda",
                columns: new[] { "BendId", "Email", "StatusPozivniceId" });
        }
    }
}
