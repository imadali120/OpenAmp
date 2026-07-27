using Microsoft.EntityFrameworkCore;
using OpenAmp.Application.Desktop;
using OpenAmp.Application.Reservations;
using OpenAmp.Domain.Entities;
using OpenAmp.Infrastructure.Persistence;

namespace OpenAmp.Infrastructure.Desktop;

public sealed class ReferenceDataService(OpenAmpDbContext dbContext) : IReferenceDataService
{
    private static readonly string[] Tipovi =
    [
        "genres",
        "instruments",
        "hall-statuses",
        "equipment-categories",
        "equipment-statuses",
        "article-categories",
        "article-statuses",
        "reservation-statuses",
        "invitation-statuses",
        "roles"
    ];

    public IReadOnlyCollection<string> PodrzaniTipovi => Tipovi;

    public Task<IReadOnlyCollection<DesktopSifarnikDto>> DohvatiAsync(
        string tip,
        string? tekst,
        CancellationToken cancellationToken = default) =>
        Normalizuj(tip) switch
        {
            "genres" => Dohvati(dbContext.Zanrovi, tekst, cancellationToken),
            "instruments" => Dohvati(dbContext.Instrumenti, tekst, cancellationToken),
            "hall-statuses" => Dohvati(dbContext.StatusiSala, tekst, cancellationToken),
            "equipment-categories" => Dohvati(dbContext.KategorijeOpreme, tekst, cancellationToken),
            "equipment-statuses" => Dohvati(dbContext.StatusiOpreme, tekst, cancellationToken),
            "article-categories" => Dohvati(dbContext.KategorijeArtikala, tekst, cancellationToken),
            "article-statuses" => Dohvati(dbContext.StatusiArtikala, tekst, cancellationToken),
            "reservation-statuses" => Dohvati(dbContext.StatusiRezervacija, tekst, cancellationToken),
            "invitation-statuses" => Dohvati(dbContext.StatusiPozivnica, tekst, cancellationToken),
            "roles" => Dohvati(dbContext.Uloge, tekst, cancellationToken),
            _ => throw NepodrzanTip()
        };

    public Task<DesktopSifarnikDto> SacuvajAsync(
        string tip,
        int? id,
        SacuvajSifarnikDto dto,
        CancellationToken cancellationToken = default) =>
        Normalizuj(tip) switch
        {
            "genres" => Sacuvaj(dbContext.Zanrovi, id, dto, cancellationToken),
            "instruments" => Sacuvaj(dbContext.Instrumenti, id, dto, cancellationToken),
            "hall-statuses" => Sacuvaj(dbContext.StatusiSala, id, dto, cancellationToken),
            "equipment-categories" => Sacuvaj(dbContext.KategorijeOpreme, id, dto, cancellationToken),
            "equipment-statuses" => Sacuvaj(dbContext.StatusiOpreme, id, dto, cancellationToken),
            "article-categories" => Sacuvaj(dbContext.KategorijeArtikala, id, dto, cancellationToken),
            "article-statuses" => Sacuvaj(dbContext.StatusiArtikala, id, dto, cancellationToken),
            "reservation-statuses" => Sacuvaj(dbContext.StatusiRezervacija, id, dto, cancellationToken),
            "invitation-statuses" => Sacuvaj(dbContext.StatusiPozivnica, id, dto, cancellationToken),
            "roles" => Sacuvaj(dbContext.Uloge, id, dto, cancellationToken),
            _ => throw NepodrzanTip()
        };

    public Task ObrisiAsync(
        string tip,
        int id,
        CancellationToken cancellationToken = default) =>
        Normalizuj(tip) switch
        {
            "genres" => Obrisi(dbContext.Zanrovi, id, cancellationToken),
            "instruments" => Obrisi(dbContext.Instrumenti, id, cancellationToken),
            "hall-statuses" => Obrisi(dbContext.StatusiSala, id, cancellationToken),
            "equipment-categories" => Obrisi(dbContext.KategorijeOpreme, id, cancellationToken),
            "equipment-statuses" => Obrisi(dbContext.StatusiOpreme, id, cancellationToken),
            "article-categories" => Obrisi(dbContext.KategorijeArtikala, id, cancellationToken),
            "article-statuses" => Obrisi(dbContext.StatusiArtikala, id, cancellationToken),
            "reservation-statuses" => Obrisi(dbContext.StatusiRezervacija, id, cancellationToken),
            "invitation-statuses" => Obrisi(dbContext.StatusiPozivnica, id, cancellationToken),
            "roles" => Obrisi(dbContext.Uloge, id, cancellationToken),
            _ => throw NepodrzanTip()
        };

    public async Task<IReadOnlyCollection<DesktopStudioDto>> DohvatiStudijeAsync(
        string? tekst,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Studiji.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(tekst))
        {
            var search = tekst.Trim();
            query = query.Where(x =>
                x.Naziv.Contains(search)
                || x.Grad.Contains(search)
                || x.Adresa.Contains(search));
        }
        return await query.OrderBy(x => x.Naziv)
            .Select(x => UDto(x))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<DesktopStudioDto> SacuvajStudioAsync(
        int? id,
        SacuvajStudioDto dto,
        CancellationToken cancellationToken = default)
    {
        ValidirajStudio(dto);
        var studio = id.HasValue
            ? await dbContext.Studiji.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new EntitetNijePronadjenException("Studio nije pronađen.")
            : new Studio();

        studio.Naziv = dto.Naziv.Trim();
        studio.Opis = Null(dto.Opis);
        studio.Adresa = dto.Adresa.Trim();
        studio.Grad = dto.Grad.Trim();
        studio.Telefon = Null(dto.Telefon);
        studio.Email = Null(dto.Email)?.ToLowerInvariant();
        studio.Aktivan = dto.Aktivan;
        studio.VremenskaZona = dto.VremenskaZona.Trim();
        studio.RadnoVrijemeOd = dto.RadnoVrijemeOd;
        studio.RadnoVrijemeDo = dto.RadnoVrijemeDo;
        studio.PuniPovratDoSati = dto.PuniPovratDoSati;
        studio.DjelimicniPovratDoSati = dto.DjelimicniPovratDoSati;
        studio.DjelimicniPovratPostotak = dto.DjelimicniPovratPostotak;
        if (!id.HasValue)
        {
            dbContext.Studiji.Add(studio);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return UDto(studio);
    }

    public async Task ObrisiStudioAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var studio = await dbContext.Studiji.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken)
            ?? throw new EntitetNijePronadjenException("Studio nije pronađen.");
        dbContext.Studiji.Remove(studio);
        await SacuvajBrisanje(cancellationToken);
    }

    private static async Task<IReadOnlyCollection<DesktopSifarnikDto>> Dohvati<TEntity>(
        DbSet<TEntity> set,
        string? tekst,
        CancellationToken cancellationToken)
        where TEntity : class, ISifarnik
    {
        var query = set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(tekst))
        {
            var search = tekst.Trim();
            query = query.Where(x => x.Kod.Contains(search) || x.Naziv.Contains(search));
        }
        return await query.OrderBy(x => x.Naziv)
            .Select(x => new DesktopSifarnikDto(x.Id, x.Kod, x.Naziv))
            .ToArrayAsync(cancellationToken);
    }

    private async Task<DesktopSifarnikDto> Sacuvaj<TEntity>(
        DbSet<TEntity> set,
        int? id,
        SacuvajSifarnikDto dto,
        CancellationToken cancellationToken)
        where TEntity : class, ISifarnik, new()
    {
        var kod = dto.Kod.Trim().ToUpperInvariant();
        var naziv = dto.Naziv.Trim();
        if (kod.Length is < 2 or > 50 || naziv.Length is < 2 or > 100)
        {
            throw new ArgumentException(
                "Kod mora imati 2–50, a naziv 2–100 znakova.");
        }
        if (await set.AnyAsync(x => x.Kod == kod && x.Id != id, cancellationToken))
        {
            throw new ArgumentException("Šifarnik sa unesenim kodom već postoji.");
        }

        var entity = id.HasValue
            ? await set.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new EntitetNijePronadjenException("Stavka šifarnika nije pronađena.")
            : new TEntity();
        entity.Kod = kod;
        entity.Naziv = naziv;
        if (!id.HasValue)
        {
            set.Add(entity);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return new DesktopSifarnikDto(entity.Id, entity.Kod, entity.Naziv);
    }

    private async Task Obrisi<TEntity>(
        DbSet<TEntity> set,
        int id,
        CancellationToken cancellationToken)
        where TEntity : class, ISifarnik
    {
        var entity = await set.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Stavka šifarnika nije pronađena.");
        set.Remove(entity);
        await SacuvajBrisanje(cancellationToken);
    }

    private async Task SacuvajBrisanje(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            dbContext.ChangeTracker.Clear();
            throw new ArgumentException(
                "Stavka se koristi u drugim podacima i ne može biti obrisana.",
                exception);
        }
    }

    private static DesktopStudioDto UDto(Studio x) => new(
        x.Id,
        x.Naziv,
        x.Opis,
        x.Adresa,
        x.Grad,
        x.Telefon,
        x.Email,
        x.Aktivan,
        x.VremenskaZona,
        x.RadnoVrijemeOd,
        x.RadnoVrijemeDo,
        x.PuniPovratDoSati,
        x.DjelimicniPovratDoSati,
        x.DjelimicniPovratPostotak);

    private static void ValidirajStudio(SacuvajStudioDto dto)
    {
        if (dto.Naziv.Trim().Length is < 2 or > 150
            || dto.Adresa.Trim().Length is < 3 or > 250
            || dto.Grad.Trim().Length is < 2 or > 100
            || dto.RadnoVrijemeDo <= dto.RadnoVrijemeOd
            || dto.PuniPovratDoSati < dto.DjelimicniPovratDoSati
            || dto.DjelimicniPovratDoSati < 0
            || dto.DjelimicniPovratPostotak is < 0 or > 100)
        {
            throw new ArgumentException("Podaci studija nisu ispravni.");
        }
        if (!string.IsNullOrWhiteSpace(dto.Email)
            && !System.Net.Mail.MailAddress.TryCreate(dto.Email.Trim(), out _))
        {
            throw new ArgumentException("Unesite ispravnu email adresu studija.");
        }
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(dto.VremenskaZona.Trim());
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException("Odabrana vremenska zona ne postoji.");
        }
        catch (InvalidTimeZoneException)
        {
            throw new ArgumentException("Odabrana vremenska zona nije ispravna.");
        }
    }

    private static string Normalizuj(string tip) => tip.Trim().ToLowerInvariant();
    private static ArgumentException NepodrzanTip() => new("Tip šifarnika nije podržan.");
    private static string? Null(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
