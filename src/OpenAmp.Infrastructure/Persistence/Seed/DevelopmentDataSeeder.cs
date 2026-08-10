using Microsoft.EntityFrameworkCore;
using OpenAmp.Application.Auth;
using OpenAmp.Domain.Entities;

namespace OpenAmp.Infrastructure.Persistence.Seed;

public sealed class DevelopmentDataSeeder(
    OpenAmpDbContext dbContext,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider)
{
    private const string ShowcaseMarker = "OPENAMP_SHOWCASE_SEED_V2";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var sada = timeProvider.GetUtcNow().UtcDateTime;
        var admin = await DodajIliAzurirajAsync(
            "admin", "Admin", "OpenAmp", "admin@openamp.local", "ADMIN", sada, cancellationToken);
        var zaposlenik = await DodajIliAzurirajAsync(
            "zaposlenik", "Studio", "Zaposlenik", "zaposlenik@openamp.local", "ZAPOSLENIK", sada, cancellationToken);
        var muzicar = await DodajIliAzurirajAsync(
            "muzicar", "Demo", "Muzičar", "muzicar@openamp.local", "MUZICAR", sada, cancellationToken);
        var jazz = await DodajIliAzurirajAsync(
            "jazz", "Jasmin", "Jazz", "jazz@openamp.local", "MUZICAR", sada, cancellationToken);
        var metal = await DodajIliAzurirajAsync(
            "metal", "Milan", "Metal", "metal@openamp.local", "MUZICAR", sada, cancellationToken);
        var sara = await DodajIliAzurirajAsync(
            "sara.vokal", "Sara", "Hadžić", "sara@openamp.local", "MUZICAR", sada, cancellationToken, "+387 61 222 410");
        var adnan = await DodajIliAzurirajAsync(
            "adnan.bas", "Adnan", "Kovač", "adnan@openamp.local", "MUZICAR", sada, cancellationToken, "+387 61 228 904");
        var lejla = await DodajIliAzurirajAsync(
            "lejla.keys", "Lejla", "Musić", "lejla@openamp.local", "MUZICAR", sada, cancellationToken);
        var tarik = await DodajIliAzurirajAsync(
            "tarik.drums", "Tarik", "Begić", "tarik@openamp.local", "MUZICAR", sada, cancellationToken);
        var nina = await DodajIliAzurirajAsync(
            "nina.guitar", "Nina", "Marić", "nina@openamp.local", "MUZICAR", sada, cancellationToken);
        var emir = await DodajIliAzurirajAsync(
            "emir.vokal", "Emir", "Salihović", "emir@openamp.local", "MUZICAR", sada, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await DodajDemoPodatkeAsync(
            admin, zaposlenik, muzicar, jazz, metal, sara, adnan, lejla, tarik, nina, emir,
            sada, cancellationToken);
    }

    private async Task<Korisnik> DodajIliAzurirajAsync(
        string username,
        string ime,
        string prezime,
        string email,
        string ulogaKod,
        DateTime sada,
        CancellationToken cancellationToken,
        string? telefon = null)
    {
        var korisnik = await dbContext.Korisnici.SingleOrDefaultAsync(
            x => x.Username == username || x.Email == email,
            cancellationToken);
        if (korisnik is not null)
        {
            korisnik.PasswordHash = passwordHasher.Hash("test");
            korisnik.Aktivan = true;
            korisnik.Telefon ??= telefon;
            return korisnik;
        }

        var uloga = await dbContext.Uloge.SingleAsync(x => x.Kod == ulogaKod, cancellationToken);
        korisnik = new Korisnik
        {
            Username = username,
            Ime = ime,
            Prezime = prezime,
            Email = email,
            PasswordHash = passwordHasher.Hash("test"),
            Telefon = telefon,
            Aktivan = true,
            KreiranUtc = sada.AddDays(-Random.Shared.Next(30, 240)),
            UlogaId = uloga.Id
        };
        dbContext.Korisnici.Add(korisnik);
        return korisnik;
    }

    private async Task DodajDemoPodatkeAsync(
        Korisnik admin,
        Korisnik zaposlenik,
        Korisnik muzicar,
        Korisnik jazz,
        Korisnik metal,
        Korisnik sara,
        Korisnik adnan,
        Korisnik lejla,
        Korisnik tarik,
        Korisnik nina,
        Korisnik emir,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        var instrumenti = await dbContext.Instrumenti.ToDictionaryAsync(x => x.Kod, cancellationToken);
        var korisnickiInstrumenti = new[]
        {
            (muzicar, "GITARA"), (jazz, "KLAVIJATURE"), (metal, "BUBNJEVI"),
            (sara, "VOKAL"), (adnan, "BAS"), (lejla, "KLAVIJATURE"),
            (tarik, "BUBNJEVI"), (nina, "GITARA"), (emir, "VOKAL")
        };
        foreach (var (korisnik, instrumentKod) in korisnickiInstrumenti)
        {
            await DodajInstrumentAsync(korisnik.Id, instrumenti[instrumentKod].Id, cancellationToken);
            await DodajPostavkeAsync(korisnik.Id, sada, cancellationToken);
        }

        var zanrovi = await dbContext.Zanrovi.ToDictionaryAsync(x => x.Kod, cancellationToken);
        var demoRock = await DodajBendAsync(
            "Demo Rock Bend", "Rock četvorka iz Mostara. Probe utorkom i vikendom.",
            muzicar, zanrovi["ROCK"].Id, instrumenti["GITARA"].Id, sada, cancellationToken);
        var blueNotes = await DodajBendAsync(
            "Blue Notes", "Jazz kvartet koji priprema autorski set i klupske nastupe.",
            jazz, zanrovi["JAZZ"].Id, instrumenti["KLAVIJATURE"].Id, sada, cancellationToken);
        var metalForge = await DodajBendAsync(
            "Metal Forge", "Metal trio fokusiran na čvrst live set i precizan ritam.",
            metal, zanrovi["METAL"].Id, instrumenti["BUBNJEVI"].Id, sada, cancellationToken);
        var neonPulse = await DodajBendAsync(
            "Neon Pulse", "Pop sastav iz Sarajeva. Trenutno završavaju set za ljetne svirke.",
            sara, zanrovi["POP"].Id, instrumenti["VOKAL"].Id, sada, cancellationToken);
        var funkDistrict = await DodajBendAsync(
            "Funk District", "Funk kvartet sa klavijaturama, basom i naglašenim grooveom.",
            lejla, zanrovi["FUNK"].Id, instrumenti["KLAVIJATURE"].Id, sada, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await DodajClanaAsync(demoRock, sara, instrumenti["VOKAL"].Id, "Vokal", sada, cancellationToken);
        await DodajClanaAsync(demoRock, adnan, instrumenti["BAS"].Id, "Bas gitara", sada, cancellationToken);
        await DodajClanaAsync(demoRock, tarik, instrumenti["BUBNJEVI"].Id, "Bubnjevi", sada, cancellationToken);
        await DodajClanaAsync(blueNotes, sara, instrumenti["VOKAL"].Id, "Vokal", sada, cancellationToken);
        await DodajClanaAsync(blueNotes, adnan, instrumenti["BAS"].Id, "Kontrabas", sada, cancellationToken);
        await DodajClanaAsync(blueNotes, tarik, instrumenti["BUBNJEVI"].Id, "Bubnjevi", sada, cancellationToken);
        await DodajClanaAsync(metalForge, nina, instrumenti["GITARA"].Id, "Gitara", sada, cancellationToken);
        await DodajClanaAsync(metalForge, adnan, instrumenti["BAS"].Id, "Bas gitara", sada, cancellationToken);
        await DodajClanaAsync(neonPulse, nina, instrumenti["GITARA"].Id, "Gitara", sada, cancellationToken);
        await DodajClanaAsync(neonPulse, adnan, instrumenti["BAS"].Id, "Bas gitara", sada, cancellationToken);
        await DodajClanaAsync(neonPulse, tarik, instrumenti["BUBNJEVI"].Id, "Bubnjevi", sada, cancellationToken);
        await DodajClanaAsync(neonPulse, lejla, instrumenti["KLAVIJATURE"].Id, "Klavijature", sada, cancellationToken);
        await DodajClanaAsync(funkDistrict, sara, instrumenti["VOKAL"].Id, "Vokal", sada, cancellationToken);
        await DodajClanaAsync(funkDistrict, adnan, instrumenti["BAS"].Id, "Bas gitara", sada, cancellationToken);
        await DodajClanaAsync(funkDistrict, tarik, instrumenti["BUBNJEVI"].Id, "Bubnjevi", sada, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await DodajInventarAsync(zaposlenik, sada, cancellationToken);
        await DodajOmiljenuAsync(muzicar.Id, 1, sada.AddDays(-14), cancellationToken);
        await DodajOmiljenuAsync(sara.Id, 3, sada.AddDays(-9), cancellationToken);
        await DodajOmiljenuAsync(adnan.Id, 1, sada.AddDays(-7), cancellationToken);
        await DodajOmiljenuAsync(lejla.Id, 2, sada.AddDays(-5), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await PostaviDemoSlikeAsync(
            admin, sara, adnan, lejla, tarik, nina, emir,
            demoRock, blueNotes, metalForge, neonPulse, funkDistrict, sada, cancellationToken);
        await DodajPozivnicuAsync(neonPulse, sara, muzicar, sada, cancellationToken);
        await DodajShowcaseRezervacijeAsync(
            demoRock, blueNotes, metalForge, neonPulse, funkDistrict,
            muzicar, jazz, metal, sara, lejla, sada, cancellationToken);
    }

    private async Task DodajInventarAsync(
        Korisnik zaposlenik,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        var kategorije = await dbContext.KategorijeOpreme.ToDictionaryAsync(x => x.Kod, cancellationToken);
        var statusi = await dbContext.StatusiOpreme.ToDictionaryAsync(x => x.Kod, cancellationToken);
        var novaOprema = new[]
        {
            new Oprema { InventarskiBroj = "OPR-MO-0003", Naziv = "Yamaha Stage Custom", Opis = "Komplet bubnjeva za jazz i pop probe.", SerijskiBroj = "YSC-DEMO-003", CijenaNajmaPoSatu = 7m, DatumNabavke = new DateOnly(2025, 5, 18), Stanje = 5, KategorijaOpremeId = kategorije["INSTRUMENT"].Id, StatusOpremeId = statusi["DOSTUPNA"].Id, SalaId = 2 },
            new Oprema { InventarskiBroj = "OPR-MO-0004", Naziv = "Nord Electro 6D", Opis = "Stage piano sa stalkom i pedalom.", SerijskiBroj = "NORD-DEMO-004", CijenaNajmaPoSatu = 8m, DatumNabavke = new DateOnly(2025, 4, 2), Stanje = 5, KategorijaOpremeId = kategorije["INSTRUMENT"].Id, StatusOpremeId = statusi["DOSTUPNA"].Id, SalaId = 2 },
            new Oprema { InventarskiBroj = "OPR-SA-0002", Naziv = "Shure Beta 58A", Opis = "Vokalni mikrofon za glasne bine.", SerijskiBroj = "B58-DEMO-002", CijenaNajmaPoSatu = 2.5m, DatumNabavke = new DateOnly(2025, 6, 12), Stanje = 4, KategorijaOpremeId = kategorije["MIKROFON"].Id, StatusOpremeId = statusi["DOSTUPNA"].Id, SalaId = 3 },
            new Oprema { InventarskiBroj = "OPR-SA-0003", Naziv = "Orange Rockerverb 50", Opis = "Cijevno gitarsko pojačalo za Stage A.", SerijskiBroj = "OR50-DEMO-003", CijenaNajmaPoSatu = 6m, DatumNabavke = new DateOnly(2025, 3, 21), Stanje = 4, KategorijaOpremeId = kategorije["POJACALO"].Id, StatusOpremeId = statusi["DOSTUPNA"].Id, SalaId = 3 },
            new Oprema { InventarskiBroj = "OPR-MO-0005", Naziv = "Radial J48 DI", Opis = "Aktivni DI box za bas i klavijature.", SerijskiBroj = "J48-DEMO-005", CijenaNajmaPoSatu = 1.5m, DatumNabavke = new DateOnly(2025, 7, 8), Stanje = 5, KategorijaOpremeId = kategorije["DODACI"].Id, StatusOpremeId = statusi["DOSTUPNA"].Id, SalaId = 1 },
            new Oprema { InventarskiBroj = "OPR-SA-0004", Naziv = "Mackie Thump 12A", Opis = "Aktivni podni monitor.", SerijskiBroj = "MACKIE-DEMO-004", CijenaNajmaPoSatu = 3m, DatumNabavke = new DateOnly(2024, 11, 4), Stanje = 3, Napomena = "Čeka zamjenu visokotonca.", KategorijaOpremeId = kategorije["DODACI"].Id, StatusOpremeId = statusi["SERVIS"].Id, SalaId = 3 }
        };
        foreach (var oprema in novaOprema)
        {
            if (!await dbContext.Oprema.AnyAsync(x => x.InventarskiBroj == oprema.InventarskiBroj, cancellationToken))
            {
                dbContext.Oprema.Add(oprema);
            }
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        var servisnaOprema = await dbContext.Oprema.SingleAsync(
            x => x.InventarskiBroj == "OPR-SA-0004", cancellationToken);
        if (!await dbContext.ServisiOpreme.AnyAsync(
            x => x.OpremaId == servisnaOprema.Id && x.ZavrsenUtc == null, cancellationToken))
        {
            dbContext.ServisiOpreme.Add(new ServisOpreme
            {
                OpremaId = servisnaOprema.Id,
                PrijavljenUtc = sada.AddDays(-3),
                OpisKvara = "Visokotonac povremeno prekida pri većoj glasnoći.",
                Trosak = 85m,
                PrijavioKorisnikId = zaposlenik.Id
            });
        }

        var kategorijeArtikala = await dbContext.KategorijeArtikala.ToDictionaryAsync(x => x.Kod, cancellationToken);
        var aktivanArtikal = await dbContext.StatusiArtikala.SingleAsync(x => x.Kod == "AKTIVAN", cancellationToken);
        var noviArtikli = new[]
        {
            new Artikal { InventarskiBroj = "ART-MO-0003", Naziv = "Palice 5A", Opis = "Par hickory palica za bubnjeve.", KolicinaNaStanju = 12, MinimalnaZaliha = 4, CijenaKupovine = 18m, KategorijaArtiklaId = kategorijeArtikala["OSTALO"].Id, StatusArtiklaId = aktivanArtikal.Id, StudioId = 1 },
            new Artikal { InventarskiBroj = "ART-MO-0004", Naziv = "Set bas žica 45-105", Opis = "Niklovane žice za četverožičani bas.", KolicinaNaStanju = 7, MinimalnaZaliha = 3, CijenaKupovine = 42m, KategorijaArtiklaId = kategorijeArtikala["ZICE"].Id, StatusArtiklaId = aktivanArtikal.Id, StudioId = 1 },
            new Artikal { InventarskiBroj = "ART-SA-0002", Naziv = "Čepići za uši", Opis = "Višekratna zaštita sluha za probe.", KolicinaNaStanju = 3, MinimalnaZaliha = 10, CijenaKupovine = 8m, KategorijaArtiklaId = kategorijeArtikala["OSTALO"].Id, StatusArtiklaId = aktivanArtikal.Id, StudioId = 2 },
            new Artikal { InventarskiBroj = "ART-SA-0003", Naziv = "Trzalica 1.00 mm", Opis = "Tvrđa trzalica za bas i gitaru.", KolicinaNaStanju = 48, MinimalnaZaliha = 15, CijenaKupovine = 1.5m, KategorijaArtiklaId = kategorijeArtikala["TRZALICE"].Id, StatusArtiklaId = aktivanArtikal.Id, StudioId = 2 }
        };
        foreach (var artikal in noviArtikli)
        {
            if (!await dbContext.Artikli.AnyAsync(x => x.InventarskiBroj == artikal.InventarskiBroj, cancellationToken))
            {
                dbContext.Artikli.Add(artikal);
            }
        }
    }

    private async Task PostaviDemoSlikeAsync(
        Korisnik admin,
        Korisnik sara,
        Korisnik adnan,
        Korisnik lejla,
        Korisnik tarik,
        Korisnik nina,
        Korisnik emir,
        Bend demoRock,
        Bend blueNotes,
        Bend metalForge,
        Bend neonPulse,
        Bend funkDistrict,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        var slike = new Dictionary<string, MedijskaDatoteka>
        {
            ["hall-marshall-room.png"] = await DodajMedijAsync("hall-marshall-room.png", admin.Id, sada, cancellationToken),
            ["hall-jazz-corner.png"] = await DodajMedijAsync("hall-jazz-corner.png", admin.Id, sada, cancellationToken),
            ["hall-stage-a.png"] = await DodajMedijAsync("hall-stage-a.png", admin.Id, sada, cancellationToken),
            ["band-demo-rock.png"] = await DodajMedijAsync("band-demo-rock.png", admin.Id, sada, cancellationToken),
            ["band-blue-notes.png"] = await DodajMedijAsync("band-blue-notes.png", admin.Id, sada, cancellationToken),
            ["band-metal-forge.png"] = await DodajMedijAsync("band-metal-forge.png", admin.Id, sada, cancellationToken),
            ["band-neon-pulse.png"] = await DodajMedijAsync("band-neon-pulse.png", sara.Id, sada, cancellationToken),
            ["band-funk-district.png"] = await DodajMedijAsync("band-funk-district.png", admin.Id, sada, cancellationToken),
            ["profile-sara-hadzic.png"] = await DodajMedijAsync("profile-sara-hadzic.png", sara.Id, sada, cancellationToken),
            ["profile-adnan-kovac.png"] = await DodajMedijAsync("profile-adnan-kovac.png", adnan.Id, sada, cancellationToken),
            ["profile-lejla-music.png"] = await DodajMedijAsync("profile-lejla-music.png", lejla.Id, sada, cancellationToken),
            ["profile-tarik-begic.png"] = await DodajMedijAsync("profile-tarik-begic.png", tarik.Id, sada, cancellationToken),
            ["profile-nina-maric.png"] = await DodajMedijAsync("profile-nina-maric.png", nina.Id, sada, cancellationToken),
            ["profile-emir-salihovic.png"] = await DodajMedijAsync("profile-emir-salihovic.png", emir.Id, sada, cancellationToken)
        };

        sara.ProfilnaSlikaId = slike["profile-sara-hadzic.png"].Id;
        sara.FotografijaUrl = SlikaUrl(sara.ProfilnaSlikaId.Value);
        adnan.ProfilnaSlikaId = slike["profile-adnan-kovac.png"].Id;
        adnan.FotografijaUrl = SlikaUrl(adnan.ProfilnaSlikaId.Value);
        PostaviProfilnuSliku(lejla, slike["profile-lejla-music.png"]);
        PostaviProfilnuSliku(tarik, slike["profile-tarik-begic.png"]);
        PostaviProfilnuSliku(nina, slike["profile-nina-maric.png"]);
        PostaviProfilnuSliku(emir, slike["profile-emir-salihovic.png"]);

        PostaviSlikuBenda(demoRock, slike["band-demo-rock.png"]);
        PostaviSlikuBenda(blueNotes, slike["band-blue-notes.png"]);
        PostaviSlikuBenda(metalForge, slike["band-metal-forge.png"]);
        PostaviSlikuBenda(neonPulse, slike["band-neon-pulse.png"]);
        PostaviSlikuBenda(funkDistrict, slike["band-funk-district.png"]);

        var sale = await dbContext.Sale.Include(x => x.Galerija).ToDictionaryAsync(x => x.Id, cancellationToken);
        PostaviSlikuSale(sale[1], slike["hall-marshall-room.png"]);
        PostaviSlikuSale(sale[2], slike["hall-jazz-corner.png"]);
        PostaviSlikuSale(sale[3], slike["hall-stage-a.png"]);

        var studiji = await dbContext.Studiji.ToDictionaryAsync(x => x.Id, cancellationToken);
        studiji[1].FotografijaId = slike["hall-marshall-room.png"].Id;
        studiji[1].FotografijaUrl = SlikaUrl(slike["hall-marshall-room.png"].Id);
        studiji[2].FotografijaId = slike["hall-stage-a.png"].Id;
        studiji[2].FotografijaUrl = SlikaUrl(slike["hall-stage-a.png"].Id);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<MedijskaDatoteka> DodajMedijAsync(
        string naziv,
        int kreiraoKorisnikId,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        var postojeci = await dbContext.MedijskeDatoteke.SingleOrDefaultAsync(
            x => x.NazivDatoteke == naziv, cancellationToken);
        if (postojeci is not null)
        {
            return postojeci;
        }

        var resourceName = $"{typeof(DevelopmentDataSeeder).Namespace}.Assets.{naziv}";
        await using var stream = typeof(DevelopmentDataSeeder).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Seed slika '{resourceName}' nije pronađena.");
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        var sadrzaj = buffer.ToArray();
        var medij = new MedijskaDatoteka
        {
            NazivDatoteke = naziv,
            ContentType = "image/png",
            Sadrzaj = sadrzaj,
            Velicina = sadrzaj.LongLength,
            KreiranaUtc = sada,
            KreiraoKorisnikId = kreiraoKorisnikId
        };
        dbContext.MedijskeDatoteke.Add(medij);
        await dbContext.SaveChangesAsync(cancellationToken);
        return medij;
    }

    private static void PostaviSlikuBenda(Bend bend, MedijskaDatoteka medij)
    {
        bend.FotografijaId = medij.Id;
        bend.FotografijaUrl = SlikaUrl(medij.Id);
    }

    private static void PostaviProfilnuSliku(Korisnik korisnik, MedijskaDatoteka medij)
    {
        korisnik.ProfilnaSlikaId = medij.Id;
        korisnik.FotografijaUrl = SlikaUrl(medij.Id);
    }

    private static void PostaviSlikuSale(Sala sala, MedijskaDatoteka medij)
    {
        var naslovna = sala.Galerija.OrderBy(x => x.Redoslijed).First();
        naslovna.MedijskaDatotekaId = medij.Id;
        naslovna.Url = SlikaUrl(medij.Id);
    }

    private async Task DodajShowcaseRezervacijeAsync(
        Bend demoRock,
        Bend blueNotes,
        Bend metalForge,
        Bend neonPulse,
        Bend funkDistrict,
        Korisnik muzicar,
        Korisnik jazz,
        Korisnik metal,
        Korisnik sara,
        Korisnik lejla,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Rezervacije.AnyAsync(x => x.Napomena == ShowcaseMarker, cancellationToken))
        {
            return;
        }

        var statusi = await dbContext.StatusiRezervacija.ToDictionaryAsync(x => x.Kod, cancellationToken);
        var rezervacije = new[]
        {
            Rezervacija(demoRock, muzicar, 1, Termin(sada, -28, 18), 120, statusi["IZVRSENA"].Id, 70m),
            Rezervacija(blueNotes, jazz, 2, Termin(sada, -24, 19), 90, statusi["IZVRSENA"].Id, 44m),
            Rezervacija(metalForge, metal, 3, Termin(sada, -19, 18), 120, statusi["IZVRSENA"].Id, 82m),
            Rezervacija(neonPulse, sara, 3, Termin(sada, -14, 20), 120, statusi["IZVRSENA"].Id, 76m),
            Rezervacija(funkDistrict, lejla, 2, Termin(sada, -9, 19), 120, statusi["IZVRSENA"].Id, 56m),
            Rezervacija(demoRock, muzicar, 1, Termin(sada, 1, 18), 120, statusi["PLACENA"].Id, 72m),
            Rezervacija(blueNotes, jazz, 2, Termin(sada, 2, 19), 90, statusi["PLACENA"].Id, 44m),
            Rezervacija(neonPulse, sara, 3, Termin(sada, 3, 18), 120, statusi["NA_CEKANJU"].Id, 78m),
            Rezervacija(funkDistrict, lejla, 2, Termin(sada, 5, 19), 120, statusi["PLACENA"].Id, 56m),
            Rezervacija(metalForge, metal, 1, Termin(sada, 7, 20), 120, statusi["NA_CEKANJU"].Id, 70m),
            Rezervacija(neonPulse, sara, 1, Termin(sada, 4, 10), 60, statusi["OTKAZANA"].Id, 30m)
        };
        rezervacije[^1].OtkazanaUtc = sada.AddDays(-1);
        rezervacije[^1].RazlogOtkazivanja = "Promjena termina nastupa";

        var marshall = await dbContext.Oprema.SingleAsync(x => x.InventarskiBroj == "OPR-MO-0001", cancellationToken);
        var mikrofon = await dbContext.Oprema.SingleAsync(x => x.InventarskiBroj == "OPR-MO-0002", cancellationToken);
        rezervacije[5].Stavke.Add(StavkaOpreme(marshall, 2));
        rezervacije[6].Stavke.Add(StavkaOpreme(mikrofon, 1.5m));
        dbContext.Rezervacije.AddRange(rezervacije);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.Recenzije.AddRange(
            Recenzija(rezervacije[0], muzicar.Id, 5, "Odličan backline i dovoljno prostora za cijeli bend.", sada.AddDays(-27)),
            Recenzija(rezervacije[1], jazz.Id, 5, "Topla akustika i klavir su bili baš ono što nam treba.", sada.AddDays(-23)),
            Recenzija(rezervacije[2], metal.Id, 4, "Stage A je prostran, monitori bi mogli biti malo glasniji.", sada.AddDays(-18)),
            Recenzija(rezervacije[3], sara.Id, 5, "Čista sala, dobra oprema i brz ulazak na termin.", sada.AddDays(-13)),
            Recenzija(rezervacije[4], lejla.Id, 4, "Ugodan prostor za manji sastav i vrlo korektna cijena.", sada.AddDays(-8)));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DodajPozivnicuAsync(
        Bend bend,
        Korisnik pozvao,
        Korisnik pozvani,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        const string kod = "OPENAMP-SHOWCASE-INVITE-NEON-PULSE";
        if (await dbContext.PozivniceBenda.AnyAsync(x => x.Kod == kod, cancellationToken))
        {
            return;
        }
        var status = await dbContext.StatusiPozivnica.SingleAsync(x => x.Kod == "NA_CEKANJU", cancellationToken);
        dbContext.PozivniceBenda.Add(new PozivnicaBenda
        {
            BendId = bend.Id,
            PozvaoKorisnikId = pozvao.Id,
            PozvaniKorisnikId = pozvani.Id,
            Username = pozvani.Username,
            Kod = kod,
            StatusPozivniceId = status.Id,
            KreiranaUtc = sada.AddHours(-6),
            IsticeUtc = sada.AddDays(7)
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DodajInstrumentAsync(int korisnikId, int instrumentId, CancellationToken cancellationToken)
    {
        if (!await dbContext.KorisnikInstrumenti.AnyAsync(
            x => x.KorisnikId == korisnikId && x.InstrumentId == instrumentId, cancellationToken))
        {
            dbContext.KorisnikInstrumenti.Add(new KorisnikInstrument
            {
                KorisnikId = korisnikId,
                InstrumentId = instrumentId,
                Primarni = true
            });
        }
    }

    private async Task DodajPostavkeAsync(int korisnikId, DateTime sada, CancellationToken cancellationToken)
    {
        if (!await dbContext.PostavkeKorisnika.AnyAsync(x => x.KorisnikId == korisnikId, cancellationToken))
        {
            dbContext.PostavkeKorisnika.Add(new PostavkeKorisnika
            {
                KorisnikId = korisnikId,
                PushNotifikacije = true,
                EmailNotifikacije = true,
                Jezik = "bs",
                ProfilJavan = true,
                AzuriraneUtc = sada
            });
        }
    }

    private async Task<Bend> DodajBendAsync(
        string naziv,
        string opis,
        Korisnik osnivac,
        int zanrId,
        int instrumentId,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        var bend = await dbContext.Bendovi.SingleOrDefaultAsync(x => x.Naziv == naziv, cancellationToken);
        if (bend is not null)
        {
            bend.Opis = opis;
            return bend;
        }
        bend = new Bend
        {
            Naziv = naziv,
            Opis = opis,
            KreiranUtc = sada.AddMonths(-Random.Shared.Next(3, 15)),
            OsnivacId = osnivac.Id,
            ZanrId = zanrId
        };
        dbContext.Bendovi.Add(bend);
        dbContext.ClanoviBenda.Add(new ClanBenda
        {
            Bend = bend,
            KorisnikId = osnivac.Id,
            InstrumentId = instrumentId,
            UlogaUBendu = "Osnivač",
            DatumPridruzivanjaUtc = bend.KreiranUtc,
            Aktivan = true
        });
        return bend;
    }

    private async Task DodajClanaAsync(
        Bend bend,
        Korisnik korisnik,
        int instrumentId,
        string uloga,
        DateTime sada,
        CancellationToken cancellationToken)
    {
        var clan = await dbContext.ClanoviBenda.SingleOrDefaultAsync(
            x => x.BendId == bend.Id && x.KorisnikId == korisnik.Id, cancellationToken);
        if (clan is null)
        {
            dbContext.ClanoviBenda.Add(new ClanBenda
            {
                BendId = bend.Id,
                KorisnikId = korisnik.Id,
                InstrumentId = instrumentId,
                UlogaUBendu = uloga,
                DatumPridruzivanjaUtc = sada.AddMonths(-2),
                Aktivan = true
            });
            return;
        }
        clan.InstrumentId = instrumentId;
        clan.UlogaUBendu = uloga;
        clan.Aktivan = true;
    }

    private async Task DodajOmiljenuAsync(
        int korisnikId,
        int salaId,
        DateTime kreirana,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.OmiljeneSale.AnyAsync(
            x => x.KorisnikId == korisnikId && x.SalaId == salaId, cancellationToken))
        {
            dbContext.OmiljeneSale.Add(new OmiljenaSala
            {
                KorisnikId = korisnikId,
                SalaId = salaId,
                KreiranaUtc = kreirana
            });
        }
    }

    private static Rezervacija Rezervacija(
        Bend bend,
        Korisnik korisnik,
        int salaId,
        DateTime termin,
        int minute,
        int statusId,
        decimal cijena) => new()
        {
            SalaId = salaId,
            BendId = bend.Id,
            KreiraoKorisnikId = korisnik.Id,
            TerminOdUtc = termin,
            TerminDoUtc = termin.AddMinutes(minute),
            UkupnaCijena = cijena,
            StatusRezervacijeId = statusId,
            Napomena = ShowcaseMarker,
            KreiranaUtc = termin.AddDays(-5),
            AzuriranaUtc = termin.AddDays(-2)
        };

    private static StavkaRezervacije StavkaOpreme(Oprema oprema, decimal sati) => new()
    {
        OpremaId = oprema.Id,
        Naziv = oprema.Naziv,
        Kolicina = 1,
        JedinicnaCijena = oprema.CijenaNajmaPoSatu,
        BrojSati = sati,
        UkupnaCijena = decimal.Round(oprema.CijenaNajmaPoSatu * sati, 2)
    };

    private static Recenzija Recenzija(
        Rezervacija rezervacija,
        int korisnikId,
        int ocjena,
        string komentar,
        DateTime kreirana) => new()
        {
            RezervacijaId = rezervacija.Id,
            KorisnikId = korisnikId,
            SalaId = rezervacija.SalaId,
            Ocjena = ocjena,
            Komentar = komentar,
            KreiranaUtc = kreirana,
            Vidljiva = true
        };

    private static DateTime Termin(DateTime sada, int pomakDana, int sat) =>
        DateTime.SpecifyKind(sada.Date.AddDays(pomakDana).AddHours(sat), DateTimeKind.Utc);

    private static string SlikaUrl(int id) => $"/api/images/{id}";
}
