using Microsoft.EntityFrameworkCore;
using OpenAmp.Application.Media;
using OpenAmp.Application.Reservations;
using OpenAmp.Domain.Entities;
using OpenAmp.Infrastructure.Persistence;

namespace OpenAmp.Infrastructure.Media;

public sealed class MediaService(
    OpenAmpDbContext dbContext,
    TimeProvider timeProvider) : IMediaService
{
    private const int MaxVelicina = 5 * 1024 * 1024;
    private static readonly HashSet<string> DozvoljeniTipovi =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    public Task<MedijskaDatotekaDto?> DohvatiAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        dbContext.MedijskeDatoteke
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new MedijskaDatotekaDto(
                x.Id,
                x.NazivDatoteke,
                x.ContentType,
                x.Sadrzaj,
                x.KreiranaUtc))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<UploadSlikeDto> PostaviProfilnuSlikuAsync(
        int korisnikId,
        NovaSlikaDto slika,
        CancellationToken cancellationToken = default)
    {
        var korisnik = await dbContext.Korisnici.SingleOrDefaultAsync(x => x.Id == korisnikId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Korisnik nije pronađen.");
        var staraId = korisnik.ProfilnaSlikaId;
        var medij = await DodajMedijAsync(korisnikId, slika, cancellationToken);
        korisnik.ProfilnaSlikaId = medij.Id;
        korisnik.FotografijaUrl = Url(medij.Id);
        await dbContext.SaveChangesAsync(cancellationToken);
        await UkloniStariAsync(staraId, medij.Id, cancellationToken);
        return new UploadSlikeDto(medij.Id, korisnik.FotografijaUrl);
    }

    public async Task<UploadSlikeDto> PostaviSlikuBendaAsync(
        int korisnikId,
        int bendId,
        NovaSlikaDto slika,
        CancellationToken cancellationToken = default)
    {
        var bend = await dbContext.Bendovi.SingleOrDefaultAsync(x => x.Id == bendId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Bend nije pronađen.");
        if (bend.OsnivacId != korisnikId)
        {
            throw new NedozvoljenaOperacijaException("Samo osnivač može promijeniti sliku benda.");
        }

        var staraId = bend.FotografijaId;
        var medij = await DodajMedijAsync(korisnikId, slika, cancellationToken);
        bend.FotografijaId = medij.Id;
        bend.FotografijaUrl = Url(medij.Id);
        await dbContext.SaveChangesAsync(cancellationToken);
        await UkloniStariAsync(staraId, medij.Id, cancellationToken);
        return new UploadSlikeDto(medij.Id, bend.FotografijaUrl);
    }

    public async Task<UploadSlikeDto> PostaviSlikuStudijaAsync(
        int korisnikId,
        int studioId,
        NovaSlikaDto slika,
        CancellationToken cancellationToken = default)
    {
        var studio = await dbContext.Studiji.SingleOrDefaultAsync(x => x.Id == studioId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Studio nije pronađen.");
        await OsigurajUpravljanjeStudijemAsync(studio, korisnikId, cancellationToken);
        var staraId = studio.FotografijaId;
        var medij = await DodajMedijAsync(korisnikId, slika, cancellationToken);
        studio.FotografijaId = medij.Id;
        studio.FotografijaUrl = Url(medij.Id);
        await dbContext.SaveChangesAsync(cancellationToken);
        await UkloniStariAsync(staraId, medij.Id, cancellationToken);
        return new UploadSlikeDto(medij.Id, studio.FotografijaUrl);
    }

    public async Task<UploadSlikeDto> DodajSlikuSaleAsync(
        int korisnikId,
        int salaId,
        NovaSlikaDto slika,
        string? alternativniTekst,
        CancellationToken cancellationToken = default)
    {
        var sala = await dbContext.Sale
            .Include(x => x.Studio)
            .Include(x => x.Galerija)
            .SingleOrDefaultAsync(x => x.Id == salaId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Sala nije pronađena.");
        await OsigurajUpravljanjeStudijemAsync(sala.Studio, korisnikId, cancellationToken);
        if (sala.Galerija.Count >= 12)
        {
            throw new InvalidOperationException("Galerija sale može imati najviše 12 slika.");
        }

        var medij = await DodajMedijAsync(korisnikId, slika, cancellationToken);
        sala.Galerija.Add(new SalaSlika
        {
            MedijskaDatotekaId = medij.Id,
            Url = Url(medij.Id),
            AlternativniTekst = string.IsNullOrWhiteSpace(alternativniTekst)
                ? $"{sala.Naziv} – fotografija"
                : alternativniTekst.Trim(),
            Redoslijed = sala.Galerija.Count == 0 ? 1 : sala.Galerija.Max(x => x.Redoslijed) + 1
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return new UploadSlikeDto(medij.Id, Url(medij.Id));
    }

    private async Task<MedijskaDatoteka> DodajMedijAsync(
        int korisnikId,
        NovaSlikaDto slika,
        CancellationToken cancellationToken)
    {
        Validiraj(slika);
        var medij = new MedijskaDatoteka
        {
            NazivDatoteke = Path.GetFileName(slika.NazivDatoteke),
            ContentType = slika.ContentType.ToLowerInvariant(),
            Sadrzaj = slika.Sadrzaj,
            Velicina = slika.Sadrzaj.LongLength,
            KreiranaUtc = timeProvider.GetUtcNow().UtcDateTime,
            KreiraoKorisnikId = korisnikId
        };
        dbContext.MedijskeDatoteke.Add(medij);
        await dbContext.SaveChangesAsync(cancellationToken);
        return medij;
    }

    private async Task OsigurajUpravljanjeStudijemAsync(
        Studio studio,
        int korisnikId,
        CancellationToken cancellationToken)
    {
        var korisnik = await dbContext.Korisnici
            .Include(x => x.Uloga)
            .SingleOrDefaultAsync(x => x.Id == korisnikId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Korisnik nije pronađen.");
        if (studio.VlasnikId != korisnikId && korisnik.Uloga.Kod is not ("ADMIN" or "ZAPOSLENIK"))
        {
            throw new NedozvoljenaOperacijaException("Nemaš dozvolu za upravljanje slikama ovog studija.");
        }
    }

    private async Task UkloniStariAsync(int? stariId, int noviId, CancellationToken cancellationToken)
    {
        if (!stariId.HasValue || stariId.Value == noviId)
        {
            return;
        }

        var stari = await dbContext.MedijskeDatoteke.SingleOrDefaultAsync(x => x.Id == stariId.Value, cancellationToken);
        if (stari is not null)
        {
            dbContext.MedijskeDatoteke.Remove(stari);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static void Validiraj(NovaSlikaDto slika)
    {
        if (slika.Sadrzaj.Length is 0 or > MaxVelicina)
        {
            throw new ArgumentException("Slika mora biti manja od 5 MB.");
        }
        if (!DozvoljeniTipovi.Contains(slika.ContentType) || !PotpisOdgovara(slika.ContentType, slika.Sadrzaj))
        {
            throw new ArgumentException("Podržani formati slika su JPEG, PNG i WebP.");
        }
    }

    private static bool PotpisOdgovara(string contentType, byte[] data) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
        "image/png" => data.Length >= 8
            && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47,
        "image/webp" => data.Length >= 12
            && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46
            && data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50,
        _ => false
    };

    private static string Url(int id) => $"/api/images/{id}";
}
