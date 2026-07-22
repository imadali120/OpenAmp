namespace OpenAmp.Infrastructure.Persistence.Seed;

internal static class OpenAmpSeed
{
    internal static readonly object[] Uloge =
    [
        new { Id = 1, Kod = "ADMIN", Naziv = "Administrator" },
        new { Id = 2, Kod = "ZAPOSLENIK", Naziv = "Zaposlenik" },
        new { Id = 3, Kod = "MUZICAR", Naziv = "Muzičar" }
    ];

    internal static readonly object[] Zanrovi =
    [
        new { Id = 1, Kod = "ROCK", Naziv = "Rock" },
        new { Id = 2, Kod = "METAL", Naziv = "Metal" },
        new { Id = 3, Kod = "JAZZ", Naziv = "Jazz" },
        new { Id = 4, Kod = "POP", Naziv = "Pop" },
        new { Id = 5, Kod = "FUNK", Naziv = "Funk" }
    ];

    internal static readonly object[] Instrumenti =
    [
        new { Id = 1, Kod = "VOKAL", Naziv = "Vokal" },
        new { Id = 2, Kod = "GITARA", Naziv = "Gitara" },
        new { Id = 3, Kod = "BAS", Naziv = "Bas gitara" },
        new { Id = 4, Kod = "BUBNJEVI", Naziv = "Bubnjevi" },
        new { Id = 5, Kod = "KLAVIJATURE", Naziv = "Klavijature" }
    ];

    internal static readonly object[] StatusiSala =
    [
        new { Id = 1, Kod = "AKTIVNA", Naziv = "Aktivna" },
        new { Id = 2, Kod = "ODRZAVANJE", Naziv = "Na održavanju" },
        new { Id = 3, Kod = "NEAKTIVNA", Naziv = "Neaktivna" }
    ];

    internal static readonly object[] KategorijeOpreme =
    [
        new { Id = 1, Kod = "POJACALO", Naziv = "Pojačalo" },
        new { Id = 2, Kod = "MIKROFON", Naziv = "Mikrofon" },
        new { Id = 3, Kod = "INSTRUMENT", Naziv = "Instrument" },
        new { Id = 4, Kod = "KABLOVI", Naziv = "Kablovi" },
        new { Id = 5, Kod = "DODACI", Naziv = "Dodaci" }
    ];

    internal static readonly object[] StatusiOpreme =
    [
        new { Id = 1, Kod = "DOSTUPNA", Naziv = "Dostupna" },
        new { Id = 2, Kod = "IZNAJMLJENA", Naziv = "Iznajmljena" },
        new { Id = 3, Kod = "SERVIS", Naziv = "Na servisu" },
        new { Id = 4, Kod = "POKVARENA", Naziv = "Pokvarena" }
    ];

    internal static readonly object[] KategorijeArtikala =
    [
        new { Id = 1, Kod = "ZICE", Naziv = "Žice" },
        new { Id = 2, Kod = "TRZALICE", Naziv = "Trzalice" },
        new { Id = 3, Kod = "BATERIJE", Naziv = "Baterije" },
        new { Id = 4, Kod = "OSTALO", Naziv = "Ostalo" }
    ];

    internal static readonly object[] StatusiArtikala =
    [
        new { Id = 1, Kod = "AKTIVAN", Naziv = "Aktivan" },
        new { Id = 2, Kod = "NEDOSTUPAN", Naziv = "Nedostupan" },
        new { Id = 3, Kod = "UKINUT", Naziv = "Ukinut" }
    ];

    internal static readonly object[] StatusiRezervacija =
    [
        new { Id = 1, Kod = "NA_CEKANJU", Naziv = "Na čekanju" },
        new { Id = 2, Kod = "PLACENA", Naziv = "Plaćena" },
        new { Id = 3, Kod = "IZVRSENA", Naziv = "Izvršena" },
        new { Id = 4, Kod = "OTKAZANA", Naziv = "Otkazana" }
    ];

    internal static readonly object[] StatusiPozivnica =
    [
        new { Id = 1, Kod = "NA_CEKANJU", Naziv = "Na čekanju" },
        new { Id = 2, Kod = "PRIHVACENA", Naziv = "Prihvaćena" },
        new { Id = 3, Kod = "ODBIJENA", Naziv = "Odbijena" },
        new { Id = 4, Kod = "ISTEKLA", Naziv = "Istekla" }
    ];

    internal static readonly object[] Studiji =
    [
        new
        {
            Id = 1,
            Naziv = "OpenAmp Mostar",
            Opis = "Testni studio za razvoj i demonstraciju sistema.",
            Adresa = "Kneza Višeslava 10",
            Grad = "Mostar",
            Telefon = "+387 36 000 001",
            Email = "mostar@example.openamp.local",
            Aktivan = true,
            VremenskaZona = "Europe/Sarajevo",
            RadnoVrijemeOd = new TimeOnly(8, 0),
            RadnoVrijemeDo = new TimeOnly(23, 0),
            PuniPovratDoSati = 24,
            DjelimicniPovratDoSati = 12,
            DjelimicniPovratPostotak = 50,
            VlasnikId = (int?)null
        },
        new
        {
            Id = 2,
            Naziv = "OpenAmp Sarajevo",
            Opis = "Drugi testni studio za provjeru rada sa više lokacija.",
            Adresa = "Zmaja od Bosne 20",
            Grad = "Sarajevo",
            Telefon = "+387 33 000 002",
            Email = "sarajevo@example.openamp.local",
            Aktivan = true,
            VremenskaZona = "Europe/Sarajevo",
            RadnoVrijemeOd = new TimeOnly(8, 0),
            RadnoVrijemeDo = new TimeOnly(23, 0),
            PuniPovratDoSati = 24,
            DjelimicniPovratDoSati = 12,
            DjelimicniPovratPostotak = 50,
            VlasnikId = (int?)null
        }
    ];

    internal static readonly object[] Sale =
    [
        new
        {
            Id = 1,
            StudioId = 1,
            Naziv = "Marshall Room",
            Kapacitet = 6,
            CijenaPoSatu = 30.00m,
            StatusSaleId = 1,
            Opis = "Sala za rock i metal probe sa kompletnim backlineom.",
            Akustika = "Akustički tretirana, kontrolisan niski spektar.",
            GeografskaSirina = (decimal?)43.3438m,
            GeografskaDuzina = (decimal?)17.8078m
        },
        new
        {
            Id = 2,
            StudioId = 1,
            Naziv = "Jazz Corner",
            Kapacitet = 4,
            CijenaPoSatu = 24.00m,
            StatusSaleId = 1,
            Opis = "Kompaktna sala sa toplijom akustikom za manje sastave.",
            Akustika = "Topao, prirodan odjek pogodan za akustične instrumente.",
            GeografskaSirina = (decimal?)43.3438m,
            GeografskaDuzina = (decimal?)17.8078m
        },
        new
        {
            Id = 3,
            StudioId = 2,
            Naziv = "Stage A",
            Kapacitet = 8,
            CijenaPoSatu = 35.00m,
            StatusSaleId = 1,
            Opis = "Velika sala pogodna za kompletne bendove i pripremu nastupa.",
            Akustika = "Neutralna i dobro prigušena.",
            GeografskaSirina = (decimal?)43.8563m,
            GeografskaDuzina = (decimal?)18.4131m
        }
    ];

    internal static readonly object[] SlikeSala =
    [
        new { Id = 1, SalaId = 1, Url = "https://example.openamp.local/images/marshall-room-1.jpg", AlternativniTekst = "Marshall Room - glavni pogled", Redoslijed = 1 },
        new { Id = 2, SalaId = 2, Url = "https://example.openamp.local/images/jazz-corner-1.jpg", AlternativniTekst = "Jazz Corner - glavni pogled", Redoslijed = 1 },
        new { Id = 3, SalaId = 3, Url = "https://example.openamp.local/images/stage-a-1.jpg", AlternativniTekst = "Stage A - glavni pogled", Redoslijed = 1 }
    ];

    internal static readonly object[] Oprema =
    [
        new
        {
            Id = 1,
            InventarskiBroj = "OPR-MO-0001",
            Naziv = "Marshall DSL40CR",
            Opis = "Gitarsko cijevno pojačalo 40 W.",
            SerijskiBroj = "TEST-DSL40-001",
            CijenaNajmaPoSatu = 5.00m,
            DatumNabavke = (DateOnly?)new DateOnly(2025, 1, 15),
            DatumZadnjegServisa = (DateOnly?)null,
            Napomena = (string?)null,
            KategorijaOpremeId = 1,
            StatusOpremeId = 1,
            SalaId = (int?)1
        },
        new
        {
            Id = 2,
            InventarskiBroj = "OPR-MO-0002",
            Naziv = "Shure SM58",
            Opis = "Dinamički vokalni mikrofon.",
            SerijskiBroj = "TEST-SM58-002",
            CijenaNajmaPoSatu = 2.00m,
            DatumNabavke = (DateOnly?)new DateOnly(2025, 2, 1),
            DatumZadnjegServisa = (DateOnly?)null,
            Napomena = (string?)null,
            KategorijaOpremeId = 2,
            StatusOpremeId = 1,
            SalaId = (int?)1
        },
        new
        {
            Id = 3,
            InventarskiBroj = "OPR-SA-0001",
            Naziv = "Fender Rumble 100",
            Opis = "Bas pojačalo 100 W.",
            SerijskiBroj = "TEST-RMB100-001",
            CijenaNajmaPoSatu = 4.00m,
            DatumNabavke = (DateOnly?)new DateOnly(2025, 3, 10),
            DatumZadnjegServisa = (DateOnly?)null,
            Napomena = (string?)null,
            KategorijaOpremeId = 1,
            StatusOpremeId = 1,
            SalaId = (int?)3
        }
    ];

    internal static readonly object[] Artikli =
    [
        new { Id = 1, InventarskiBroj = "ART-MO-0001", Naziv = "Set žica 10-46", Opis = "Set žica za električnu gitaru.", KolicinaNaStanju = 20, MinimalnaZaliha = 5, CijenaKupovine = 15.00m, KategorijaArtiklaId = 1, StatusArtiklaId = 1, StudioId = 1 },
        new { Id = 2, InventarskiBroj = "ART-MO-0002", Naziv = "Trzalica 0.88 mm", Opis = "Standardna najlonska trzalica.", KolicinaNaStanju = 100, MinimalnaZaliha = 20, CijenaKupovine = 1.00m, KategorijaArtiklaId = 2, StatusArtiklaId = 1, StudioId = 1 },
        new { Id = 3, InventarskiBroj = "ART-SA-0001", Naziv = "9V baterija", Opis = "Alkalna baterija za pedale.", KolicinaNaStanju = 30, MinimalnaZaliha = 8, CijenaKupovine = 6.00m, KategorijaArtiklaId = 3, StatusArtiklaId = 1, StudioId = 2 }
    ];
}
