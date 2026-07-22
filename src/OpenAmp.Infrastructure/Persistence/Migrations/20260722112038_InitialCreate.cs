using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenAmp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instrumenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrumenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KategorijeArtikala",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KategorijeArtikala", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KategorijeOpreme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KategorijeOpreme", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusiArtikala",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusiArtikala", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusiOpreme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusiOpreme", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusiPozivnica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusiPozivnica", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusiRezervacija",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusiRezervacija", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusiSala",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusiSala", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Uloge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uloge", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zanrovi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zanrovi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Korisnici",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FotografijaUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Aktivan = table.Column<bool>(type: "bit", nullable: false),
                    KreiranUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    UlogaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnici", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Korisnici_Uloge_UlogaId",
                        column: x => x.UlogaId,
                        principalTable: "Uloge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bendovi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FotografijaUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    KreiranUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    OsnivacId = table.Column<int>(type: "int", nullable: false),
                    ZanrId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bendovi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bendovi_Korisnici_OsnivacId",
                        column: x => x.OsnivacId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bendovi_Zanrovi_ZanrId",
                        column: x => x.ZanrId,
                        principalTable: "Zanrovi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KorisnikInstrumenti",
                columns: table => new
                {
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    Primarni = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KorisnikInstrumenti", x => new { x.KorisnikId, x.InstrumentId });
                    table.ForeignKey(
                        name: "FK_KorisnikInstrumenti_Instrumenti_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrumenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KorisnikInstrumenti_Korisnici_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Studiji",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Adresa = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Grad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    Aktivan = table.Column<bool>(type: "bit", nullable: false),
                    VlasnikId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studiji", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Studiji_Korisnici_VlasnikId",
                        column: x => x.VlasnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClanoviBenda",
                columns: table => new
                {
                    BendId = table.Column<int>(type: "int", nullable: false),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    InstrumentId = table.Column<int>(type: "int", nullable: true),
                    UlogaUBendu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DatumPridruzivanjaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Aktivan = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanoviBenda", x => new { x.BendId, x.KorisnikId });
                    table.ForeignKey(
                        name: "FK_ClanoviBenda_Bendovi_BendId",
                        column: x => x.BendId,
                        principalTable: "Bendovi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClanoviBenda_Instrumenti_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrumenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClanoviBenda_Korisnici_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PozivniceBenda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BendId = table.Column<int>(type: "int", nullable: false),
                    PozvaoKorisnikId = table.Column<int>(type: "int", nullable: false),
                    PozvaniKorisnikId = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StatusPozivniceId = table.Column<int>(type: "int", nullable: false),
                    KreiranaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    IsticeUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    OdgovorenaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PozivniceBenda", x => x.Id);
                    table.CheckConstraint("CK_PozivniceBenda_Datum", "[IsticeUtc] > [KreiranaUtc]");
                    table.ForeignKey(
                        name: "FK_PozivniceBenda_Bendovi_BendId",
                        column: x => x.BendId,
                        principalTable: "Bendovi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PozivniceBenda_Korisnici_PozvaniKorisnikId",
                        column: x => x.PozvaniKorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PozivniceBenda_Korisnici_PozvaoKorisnikId",
                        column: x => x.PozvaoKorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PozivniceBenda_StatusiPozivnica_StatusPozivniceId",
                        column: x => x.StatusPozivniceId,
                        principalTable: "StatusiPozivnica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Artikli",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventarskiBroj = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    KolicinaNaStanju = table.Column<int>(type: "int", nullable: false),
                    MinimalnaZaliha = table.Column<int>(type: "int", nullable: false),
                    CijenaKupovine = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    KategorijaArtiklaId = table.Column<int>(type: "int", nullable: false),
                    StatusArtiklaId = table.Column<int>(type: "int", nullable: false),
                    StudioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artikli", x => x.Id);
                    table.CheckConstraint("CK_Artikli_Cijena", "[CijenaKupovine] >= 0");
                    table.CheckConstraint("CK_Artikli_Zalihe", "[KolicinaNaStanju] >= 0 AND [MinimalnaZaliha] >= 0");
                    table.ForeignKey(
                        name: "FK_Artikli_KategorijeArtikala_KategorijaArtiklaId",
                        column: x => x.KategorijaArtiklaId,
                        principalTable: "KategorijeArtikala",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Artikli_StatusiArtikala_StatusArtiklaId",
                        column: x => x.StatusArtiklaId,
                        principalTable: "StatusiArtikala",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Artikli_Studiji_StudioId",
                        column: x => x.StudioId,
                        principalTable: "Studiji",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudioId = table.Column<int>(type: "int", nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Kapacitet = table.Column<int>(type: "int", nullable: false),
                    CijenaPoSatu = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    StatusSaleId = table.Column<int>(type: "int", nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Akustika = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GeografskaSirina = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    GeografskaDuzina = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sale", x => x.Id);
                    table.CheckConstraint("CK_Sale_CijenaPoSatu", "[CijenaPoSatu] >= 0");
                    table.CheckConstraint("CK_Sale_Kapacitet", "[Kapacitet] > 0");
                    table.ForeignKey(
                        name: "FK_Sale_StatusiSala_StatusSaleId",
                        column: x => x.StatusSaleId,
                        principalTable: "StatusiSala",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sale_Studiji_StudioId",
                        column: x => x.StudioId,
                        principalTable: "Studiji",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Oprema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventarskiBroj = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SerijskiBroj = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CijenaNajmaPoSatu = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DatumNabavke = table.Column<DateOnly>(type: "date", nullable: true),
                    DatumZadnjegServisa = table.Column<DateOnly>(type: "date", nullable: true),
                    Napomena = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    KategorijaOpremeId = table.Column<int>(type: "int", nullable: false),
                    StatusOpremeId = table.Column<int>(type: "int", nullable: false),
                    SalaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oprema", x => x.Id);
                    table.CheckConstraint("CK_Oprema_CijenaNajma", "[CijenaNajmaPoSatu] >= 0");
                    table.ForeignKey(
                        name: "FK_Oprema_KategorijeOpreme_KategorijaOpremeId",
                        column: x => x.KategorijaOpremeId,
                        principalTable: "KategorijeOpreme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Oprema_Sale_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Sale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Oprema_StatusiOpreme_StatusOpremeId",
                        column: x => x.StatusOpremeId,
                        principalTable: "StatusiOpreme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rezervacije",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaId = table.Column<int>(type: "int", nullable: false),
                    BendId = table.Column<int>(type: "int", nullable: false),
                    KreiraoKorisnikId = table.Column<int>(type: "int", nullable: false),
                    TerminOdUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    TerminDoUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    UkupnaCijena = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    StatusRezervacijeId = table.Column<int>(type: "int", nullable: false),
                    Napomena = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    KreiranaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    AzuriranaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezervacije", x => x.Id);
                    table.CheckConstraint("CK_Rezervacije_Cijena", "[UkupnaCijena] >= 0");
                    table.CheckConstraint("CK_Rezervacije_Termin", "[TerminDoUtc] > [TerminOdUtc]");
                    table.ForeignKey(
                        name: "FK_Rezervacije_Bendovi_BendId",
                        column: x => x.BendId,
                        principalTable: "Bendovi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rezervacije_Korisnici_KreiraoKorisnikId",
                        column: x => x.KreiraoKorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rezervacije_Sale_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Sale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rezervacije_StatusiRezervacija_StatusRezervacijeId",
                        column: x => x.StatusRezervacijeId,
                        principalTable: "StatusiRezervacija",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SlikeSala",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalaId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    AlternativniTekst = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Redoslijed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlikeSala", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlikeSala_Sale_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Sale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recenzije",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ocjena = table.Column<int>(type: "int", nullable: false),
                    Komentar = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    KreiranaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Vidljiva = table.Column<bool>(type: "bit", nullable: false),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    SalaId = table.Column<int>(type: "int", nullable: false),
                    RezervacijaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recenzije", x => x.Id);
                    table.CheckConstraint("CK_Recenzije_Ocjena", "[Ocjena] BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_Recenzije_Korisnici_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recenzije_Rezervacije_RezervacijaId",
                        column: x => x.RezervacijaId,
                        principalTable: "Rezervacije",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recenzije_Sale_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Sale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StavkeRezervacija",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RezervacijaId = table.Column<int>(type: "int", nullable: false),
                    OpremaId = table.Column<int>(type: "int", nullable: true),
                    ArtikalId = table.Column<int>(type: "int", nullable: true),
                    Naziv = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Kolicina = table.Column<int>(type: "int", nullable: false),
                    JedinicnaCijena = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    BrojSati = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    UkupnaCijena = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StavkeRezervacija", x => x.Id);
                    table.CheckConstraint("CK_StavkeRezervacija_Cijene", "[JedinicnaCijena] >= 0 AND [BrojSati] >= 0 AND [UkupnaCijena] >= 0");
                    table.CheckConstraint("CK_StavkeRezervacija_Kolicina", "[Kolicina] > 0");
                    table.CheckConstraint("CK_StavkeRezervacija_Tip", "([OpremaId] IS NOT NULL AND [ArtikalId] IS NULL) OR ([OpremaId] IS NULL AND [ArtikalId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_StavkeRezervacija_Artikli_ArtikalId",
                        column: x => x.ArtikalId,
                        principalTable: "Artikli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StavkeRezervacija_Oprema_OpremaId",
                        column: x => x.OpremaId,
                        principalTable: "Oprema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StavkeRezervacija_Rezervacije_RezervacijaId",
                        column: x => x.RezervacijaId,
                        principalTable: "Rezervacije",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Instrumenti",
                columns: new[] { "Id", "Kod", "Naziv" },
                values: new object[,]
                {
                    { 1, "VOKAL", "Vokal" },
                    { 2, "GITARA", "Gitara" },
                    { 3, "BAS", "Bas gitara" },
                    { 4, "BUBNJEVI", "Bubnjevi" },
                    { 5, "KLAVIJATURE", "Klavijature" }
                });

            migrationBuilder.InsertData(
                table: "KategorijeArtikala",
                columns: new[] { "Id", "Kod", "Naziv" },
                values: new object[,]
                {
                    { 1, "ZICE", "Žice" },
                    { 2, "TRZALICE", "Trzalice" },
                    { 3, "BATERIJE", "Baterije" },
                    { 4, "OSTALO", "Ostalo" }
                });

            migrationBuilder.InsertData(
                table: "KategorijeOpreme",
                columns: new[] { "Id", "Kod", "Naziv" },
                values: new object[,]
                {
                    { 1, "POJACALO", "Pojačalo" },
                    { 2, "MIKROFON", "Mikrofon" },
                    { 3, "INSTRUMENT", "Instrument" },
                    { 4, "KABLOVI", "Kablovi" },
                    { 5, "DODACI", "Dodaci" }
                });

            migrationBuilder.InsertData(
                table: "StatusiArtikala",
                columns: new[] { "Id", "Kod", "Naziv" },
                values: new object[,]
                {
                    { 1, "AKTIVAN", "Aktivan" },
                    { 2, "NEDOSTUPAN", "Nedostupan" },
                    { 3, "UKINUT", "Ukinut" }
                });

            migrationBuilder.InsertData(
                table: "StatusiOpreme",
                columns: new[] { "Id", "Kod", "Naziv" },
                values: new object[,]
                {
                    { 1, "DOSTUPNA", "Dostupna" },
                    { 2, "IZNAJMLJENA", "Iznajmljena" },
                    { 3, "SERVIS", "Na servisu" },
                    { 4, "POKVARENA", "Pokvarena" }
                });

            migrationBuilder.InsertData(
                table: "StatusiPozivnica",
                columns: new[] { "Id", "Kod", "Naziv" },
                values: new object[,]
                {
                    { 1, "NA_CEKANJU", "Na čekanju" },
                    { 2, "PRIHVACENA", "Prihvaćena" },
                    { 3, "ODBIJENA", "Odbijena" },
                    { 4, "ISTEKLA", "Istekla" }
                });

            migrationBuilder.InsertData(
                table: "StatusiRezervacija",
                columns: new[] { "Id", "Kod", "Naziv" },
                values: new object[,]
                {
                    { 1, "NA_CEKANJU", "Na čekanju" },
                    { 2, "PLACENA", "Plaćena" },
                    { 3, "IZVRSENA", "Izvršena" },
                    { 4, "OTKAZANA", "Otkazana" }
                });

            migrationBuilder.InsertData(
                table: "StatusiSala",
                columns: new[] { "Id", "Kod", "Naziv" },
                values: new object[,]
                {
                    { 1, "AKTIVNA", "Aktivna" },
                    { 2, "ODRZAVANJE", "Na održavanju" },
                    { 3, "NEAKTIVNA", "Neaktivna" }
                });

            migrationBuilder.InsertData(
                table: "Studiji",
                columns: new[] { "Id", "Adresa", "Aktivan", "Email", "Grad", "Naziv", "Opis", "Telefon", "VlasnikId" },
                values: new object[,]
                {
                    { 1, "Kneza Višeslava 10", true, "mostar@example.openamp.local", "Mostar", "OpenAmp Mostar", "Testni studio za razvoj i demonstraciju sistema.", "+387 36 000 001", null },
                    { 2, "Zmaja od Bosne 20", true, "sarajevo@example.openamp.local", "Sarajevo", "OpenAmp Sarajevo", "Drugi testni studio za provjeru rada sa više lokacija.", "+387 33 000 002", null }
                });

            migrationBuilder.InsertData(
                table: "Uloge",
                columns: new[] { "Id", "Kod", "Naziv" },
                values: new object[,]
                {
                    { 1, "ADMIN", "Administrator" },
                    { 2, "ZAPOSLENIK", "Zaposlenik" },
                    { 3, "MUZICAR", "Muzičar" }
                });

            migrationBuilder.InsertData(
                table: "Zanrovi",
                columns: new[] { "Id", "Kod", "Naziv" },
                values: new object[,]
                {
                    { 1, "ROCK", "Rock" },
                    { 2, "METAL", "Metal" },
                    { 3, "JAZZ", "Jazz" },
                    { 4, "POP", "Pop" },
                    { 5, "FUNK", "Funk" }
                });

            migrationBuilder.InsertData(
                table: "Artikli",
                columns: new[] { "Id", "CijenaKupovine", "InventarskiBroj", "KategorijaArtiklaId", "KolicinaNaStanju", "MinimalnaZaliha", "Naziv", "Opis", "StatusArtiklaId", "StudioId" },
                values: new object[,]
                {
                    { 1, 15.00m, "ART-MO-0001", 1, 20, 5, "Set žica 10-46", "Set žica za električnu gitaru.", 1, 1 },
                    { 2, 1.00m, "ART-MO-0002", 2, 100, 20, "Trzalica 0.88 mm", "Standardna najlonska trzalica.", 1, 1 },
                    { 3, 6.00m, "ART-SA-0001", 3, 30, 8, "9V baterija", "Alkalna baterija za pedale.", 1, 2 }
                });

            migrationBuilder.InsertData(
                table: "Sale",
                columns: new[] { "Id", "Akustika", "CijenaPoSatu", "GeografskaDuzina", "GeografskaSirina", "Kapacitet", "Naziv", "Opis", "StatusSaleId", "StudioId" },
                values: new object[,]
                {
                    { 1, "Akustički tretirana, kontrolisan niski spektar.", 30.00m, 17.8078m, 43.3438m, 6, "Marshall Room", "Sala za rock i metal probe sa kompletnim backlineom.", 1, 1 },
                    { 2, "Topao, prirodan odjek pogodan za akustične instrumente.", 24.00m, 17.8078m, 43.3438m, 4, "Jazz Corner", "Kompaktna sala sa toplijom akustikom za manje sastave.", 1, 1 },
                    { 3, "Neutralna i dobro prigušena.", 35.00m, 18.4131m, 43.8563m, 8, "Stage A", "Velika sala pogodna za kompletne bendove i pripremu nastupa.", 1, 2 }
                });

            migrationBuilder.InsertData(
                table: "Oprema",
                columns: new[] { "Id", "CijenaNajmaPoSatu", "DatumNabavke", "DatumZadnjegServisa", "InventarskiBroj", "KategorijaOpremeId", "Napomena", "Naziv", "Opis", "SalaId", "SerijskiBroj", "StatusOpremeId" },
                values: new object[,]
                {
                    { 1, 5.00m, new DateOnly(2025, 1, 15), null, "OPR-MO-0001", 1, null, "Marshall DSL40CR", "Gitarsko cijevno pojačalo 40 W.", 1, "TEST-DSL40-001", 1 },
                    { 2, 2.00m, new DateOnly(2025, 2, 1), null, "OPR-MO-0002", 2, null, "Shure SM58", "Dinamički vokalni mikrofon.", 1, "TEST-SM58-002", 1 },
                    { 3, 4.00m, new DateOnly(2025, 3, 10), null, "OPR-SA-0001", 1, null, "Fender Rumble 100", "Bas pojačalo 100 W.", 3, "TEST-RMB100-001", 1 }
                });

            migrationBuilder.InsertData(
                table: "SlikeSala",
                columns: new[] { "Id", "AlternativniTekst", "Redoslijed", "SalaId", "Url" },
                values: new object[,]
                {
                    { 1, "Marshall Room - glavni pogled", 1, 1, "https://example.openamp.local/images/marshall-room-1.jpg" },
                    { 2, "Jazz Corner - glavni pogled", 1, 2, "https://example.openamp.local/images/jazz-corner-1.jpg" },
                    { 3, "Stage A - glavni pogled", 1, 3, "https://example.openamp.local/images/stage-a-1.jpg" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artikli_InventarskiBroj",
                table: "Artikli",
                column: "InventarskiBroj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artikli_KategorijaArtiklaId",
                table: "Artikli",
                column: "KategorijaArtiklaId");

            migrationBuilder.CreateIndex(
                name: "IX_Artikli_StatusArtiklaId",
                table: "Artikli",
                column: "StatusArtiklaId");

            migrationBuilder.CreateIndex(
                name: "IX_Artikli_StudioId_KolicinaNaStanju",
                table: "Artikli",
                columns: new[] { "StudioId", "KolicinaNaStanju" });

            migrationBuilder.CreateIndex(
                name: "IX_Bendovi_Naziv",
                table: "Bendovi",
                column: "Naziv");

            migrationBuilder.CreateIndex(
                name: "IX_Bendovi_OsnivacId",
                table: "Bendovi",
                column: "OsnivacId");

            migrationBuilder.CreateIndex(
                name: "IX_Bendovi_ZanrId",
                table: "Bendovi",
                column: "ZanrId");

            migrationBuilder.CreateIndex(
                name: "IX_ClanoviBenda_InstrumentId",
                table: "ClanoviBenda",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClanoviBenda_KorisnikId",
                table: "ClanoviBenda",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Instrumenti_Kod",
                table: "Instrumenti",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KategorijeArtikala_Kod",
                table: "KategorijeArtikala",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KategorijeOpreme_Kod",
                table: "KategorijeOpreme",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_Email",
                table: "Korisnici",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_UlogaId",
                table: "Korisnici",
                column: "UlogaId");

            migrationBuilder.CreateIndex(
                name: "IX_KorisnikInstrumenti_InstrumentId",
                table: "KorisnikInstrumenti",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Oprema_InventarskiBroj",
                table: "Oprema",
                column: "InventarskiBroj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Oprema_KategorijaOpremeId_StatusOpremeId_SalaId",
                table: "Oprema",
                columns: new[] { "KategorijaOpremeId", "StatusOpremeId", "SalaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Oprema_SalaId",
                table: "Oprema",
                column: "SalaId");

            migrationBuilder.CreateIndex(
                name: "IX_Oprema_StatusOpremeId",
                table: "Oprema",
                column: "StatusOpremeId");

            migrationBuilder.CreateIndex(
                name: "IX_PozivniceBenda_BendId_Email_StatusPozivniceId",
                table: "PozivniceBenda",
                columns: new[] { "BendId", "Email", "StatusPozivniceId" });

            migrationBuilder.CreateIndex(
                name: "IX_PozivniceBenda_Kod",
                table: "PozivniceBenda",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PozivniceBenda_PozvaniKorisnikId",
                table: "PozivniceBenda",
                column: "PozvaniKorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_PozivniceBenda_PozvaoKorisnikId",
                table: "PozivniceBenda",
                column: "PozvaoKorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_PozivniceBenda_StatusPozivniceId",
                table: "PozivniceBenda",
                column: "StatusPozivniceId");

            migrationBuilder.CreateIndex(
                name: "IX_Recenzije_KorisnikId",
                table: "Recenzije",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Recenzije_RezervacijaId",
                table: "Recenzije",
                column: "RezervacijaId",
                unique: true,
                filter: "[RezervacijaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Recenzije_SalaId_KreiranaUtc",
                table: "Recenzije",
                columns: new[] { "SalaId", "KreiranaUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacije_BendId",
                table: "Rezervacije",
                column: "BendId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacije_KreiraoKorisnikId",
                table: "Rezervacije",
                column: "KreiraoKorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacije_Sala_Termin",
                table: "Rezervacije",
                columns: new[] { "SalaId", "TerminOdUtc", "TerminDoUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacije_StatusRezervacijeId",
                table: "Rezervacije",
                column: "StatusRezervacijeId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacije_StripePaymentIntentId",
                table: "Rezervacije",
                column: "StripePaymentIntentId",
                unique: true,
                filter: "[StripePaymentIntentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_StatusSaleId",
                table: "Sale",
                column: "StatusSaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_StudioId_Naziv",
                table: "Sale",
                columns: new[] { "StudioId", "Naziv" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlikeSala_SalaId_Redoslijed",
                table: "SlikeSala",
                columns: new[] { "SalaId", "Redoslijed" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusiArtikala_Kod",
                table: "StatusiArtikala",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusiOpreme_Kod",
                table: "StatusiOpreme",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusiPozivnica_Kod",
                table: "StatusiPozivnica",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusiRezervacija_Kod",
                table: "StatusiRezervacija",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusiSala_Kod",
                table: "StatusiSala",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StavkeRezervacija_ArtikalId",
                table: "StavkeRezervacija",
                column: "ArtikalId");

            migrationBuilder.CreateIndex(
                name: "IX_StavkeRezervacija_OpremaId",
                table: "StavkeRezervacija",
                column: "OpremaId");

            migrationBuilder.CreateIndex(
                name: "IX_StavkeRezervacija_RezervacijaId_OpremaId",
                table: "StavkeRezervacija",
                columns: new[] { "RezervacijaId", "OpremaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Studiji_Grad_Naziv",
                table: "Studiji",
                columns: new[] { "Grad", "Naziv" });

            migrationBuilder.CreateIndex(
                name: "IX_Studiji_VlasnikId",
                table: "Studiji",
                column: "VlasnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Uloge_Kod",
                table: "Uloge",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zanrovi_Kod",
                table: "Zanrovi",
                column: "Kod",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClanoviBenda");

            migrationBuilder.DropTable(
                name: "KorisnikInstrumenti");

            migrationBuilder.DropTable(
                name: "PozivniceBenda");

            migrationBuilder.DropTable(
                name: "Recenzije");

            migrationBuilder.DropTable(
                name: "SlikeSala");

            migrationBuilder.DropTable(
                name: "StavkeRezervacija");

            migrationBuilder.DropTable(
                name: "Instrumenti");

            migrationBuilder.DropTable(
                name: "StatusiPozivnica");

            migrationBuilder.DropTable(
                name: "Artikli");

            migrationBuilder.DropTable(
                name: "Oprema");

            migrationBuilder.DropTable(
                name: "Rezervacije");

            migrationBuilder.DropTable(
                name: "KategorijeArtikala");

            migrationBuilder.DropTable(
                name: "StatusiArtikala");

            migrationBuilder.DropTable(
                name: "KategorijeOpreme");

            migrationBuilder.DropTable(
                name: "StatusiOpreme");

            migrationBuilder.DropTable(
                name: "Bendovi");

            migrationBuilder.DropTable(
                name: "Sale");

            migrationBuilder.DropTable(
                name: "StatusiRezervacija");

            migrationBuilder.DropTable(
                name: "Zanrovi");

            migrationBuilder.DropTable(
                name: "StatusiSala");

            migrationBuilder.DropTable(
                name: "Studiji");

            migrationBuilder.DropTable(
                name: "Korisnici");

            migrationBuilder.DropTable(
                name: "Uloge");
        }
    }
}
