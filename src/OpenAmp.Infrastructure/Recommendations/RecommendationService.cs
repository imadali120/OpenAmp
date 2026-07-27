using Microsoft.EntityFrameworkCore;
using OpenAmp.Application.Mobile;
using OpenAmp.Application.Recommendations;
using OpenAmp.Application.Reservations;
using OpenAmp.Domain.Entities;
using OpenAmp.Infrastructure.Persistence;

namespace OpenAmp.Infrastructure.Recommendations;

public sealed class RecommendationService(
    OpenAmpDbContext dbContext) : IRecommendationService
{
    private static readonly string[] AktivniStatusi = ["NA_CEKANJU", "PLACENA"];

    public async Task<IReadOnlyCollection<SalaPreporukaDto>> PreporuciSaleAsync(
        RecommendationFilter filter,
        CancellationToken cancellationToken = default)
    {
        Validiraj(filter);
        var band = await dbContext.Bendovi
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Zanr)
            .Include(x => x.Clanovi).ThenInclude(x => x.Instrument)
            .Include(x => x.Rezervacije).ThenInclude(x => x.Sala)
            .SingleOrDefaultAsync(x => x.Id == filter.BendId, cancellationToken)
            ?? throw new EntitetNijePronadjenException("Bend nije pronađen.");

        if (band.OsnivacId != filter.KorisnikId
            && !band.Clanovi.Any(x => x.KorisnikId == filter.KorisnikId && x.Aktivan))
        {
            throw new UnauthorizedAccessException("Korisnik nije član odabranog benda.");
        }

        var zanrovi = await dbContext.Zanrovi.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new Sifarnik(x.Id, x.Kod))
            .ToArrayAsync(cancellationToken);
        var kategorije = await dbContext.KategorijeOpreme.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new Sifarnik(x.Id, x.Kod))
            .ToArrayAsync(cancellationToken);

        var saleQuery = dbContext.Sale
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Studio)
            .Include(x => x.Status)
            .Include(x => x.Galerija)
            .Include(x => x.Oprema).ThenInclude(x => x.Kategorija)
            .Include(x => x.Oprema).ThenInclude(x => x.Status)
            .Include(x => x.Recenzije)
            .Include(x => x.Rezervacije).ThenInclude(x => x.Status)
            .Include(x => x.Rezervacije).ThenInclude(x => x.Bend).ThenInclude(x => x.Zanr)
            .Where(x => x.Studio.Aktivan && x.Status.Kod == "AKTIVNA");

        if (filter.MinimalniKapacitet is > 0)
        {
            saleQuery = saleQuery.Where(x => x.Kapacitet >= filter.MinimalniKapacitet);
        }
        if (!string.IsNullOrWhiteSpace(filter.KategorijaOpremeKod))
        {
            var kategorija = filter.KategorijaOpremeKod.Trim().ToUpperInvariant();
            saleQuery = saleQuery.Where(x =>
                x.Oprema.Any(o => o.Kategorija.Kod == kategorija && o.Status.Kod == "DOSTUPNA"));
        }
        if (!string.IsNullOrWhiteSpace(filter.ZanrKod))
        {
            var zanr = filter.ZanrKod.Trim().ToUpperInvariant();
            saleQuery = saleQuery.Where(x =>
                x.Rezervacije.Count == 0
                || x.Rezervacije.Any(r => r.Status.Kod != "OTKAZANA" && r.Bend.Zanr.Kod == zanr));
        }

        var aktivneSaleBenda = await dbContext.Rezervacije.AsNoTracking()
            .Where(x => x.BendId == filter.BendId && AktivniStatusi.Contains(x.Status.Kod))
            .Select(x => x.SalaId)
            .Distinct()
            .ToArrayAsync(cancellationToken);
        saleQuery = saleQuery.Where(x => !aktivneSaleBenda.Contains(x.Id));

        var sale = await saleQuery.OrderBy(x => x.Id).ToArrayAsync(cancellationToken);
        if (filter.TerminOdUtc.HasValue)
        {
            sale = sale.Where(x => !x.Rezervacije.Any(r =>
                    r.Status.Kod != "OTKAZANA"
                    && r.TerminOdUtc < filter.TerminDoUtc
                    && filter.TerminOdUtc < r.TerminDoUtc))
                .ToArray();
        }

        var rawInteractions = await dbContext.Rezervacije.AsNoTracking()
            .Where(x => x.Status.Kod != "OTKAZANA")
            .Select(x => new
            {
                x.BendId,
                x.SalaId,
                Ocjena = x.Recenzija == null ? (int?)null : x.Recenzija.Ocjena
            })
            .ToArrayAsync(cancellationToken);
        var interactions = rawInteractions
            .GroupBy(x => new { x.BendId, x.SalaId })
            .Select(group =>
            {
                var frequency = Math.Min(
                    1,
                    0.45 + Math.Log2(group.Count() + 1) * 0.18);
                var ratings = group.Where(x => x.Ocjena.HasValue)
                    .Select(x => x.Ocjena!.Value / 5d)
                    .ToArray();
                var preference = ratings.Length == 0
                    ? frequency
                    : ratings.Average() * 0.75 + frequency * 0.25;
                return new RecommendationInteraction(
                    group.Key.BendId,
                    group.Key.SalaId,
                    preference);
            })
            .ToArray();

        var bandVector = BuildBandVector(band, sale, zanrovi, kategorije);
        var candidateVectors = sale.Select(x => new RecommendationCandidate(
            x.Id,
            BuildHallVector(x, zanrovi, kategorije))).ToArray();
        var ranked = RecommendationEngine.Rank(
            band.Id,
            bandVector,
            candidateVectors,
            interactions,
            Math.Clamp(filter.Limit, 1, 20));
        var saleById = sale.ToDictionary(x => x.Id);

        return ranked.Select(score =>
        {
            var sala = saleById[score.SalaId];
            return new SalaPreporukaDto(
                UCardDto(sala),
                Zaokruzi(score.Score),
                Zaokruzi(score.ContentScore),
                Zaokruzi(score.CollaborativeScore),
                Zaokruzi(score.Alpha),
                Objasni(score, band.Rezervacije.Count));
        }).ToArray();
    }

    private static double[] BuildHallVector(
        Sala sala,
        IReadOnlyList<Sifarnik> zanrovi,
        IReadOnlyList<Sifarnik> kategorije)
    {
        var vector = new List<double>();
        vector.AddRange(Bucket(sala.Kapacitet, 5, 9));
        vector.AddRange(Bucket((double)sala.CijenaPoSatu, 20, 35));

        var validReservations = sala.Rezervacije
            .Where(x => x.Status.Kod != "OTKAZANA")
            .ToArray();
        var reservationCount = Math.Max(1, validReservations.Length);
        vector.AddRange(zanrovi.Select(genre =>
            validReservations.Count(x => x.Bend.ZanrId == genre.Id) / (double)reservationCount));
        vector.AddRange(kategorije.Select(category =>
            sala.Oprema.Any(x =>
                x.KategorijaOpremeId == category.Id
                && x.Status.Kod == "DOSTUPNA") ? 1d : 0d));
        vector.AddRange(AcousticFeatures(sala));
        return [.. vector];
    }

    private static double[] BuildBandVector(
        Bend band,
        IReadOnlyCollection<Sala> candidateHalls,
        IReadOnlyList<Sifarnik> zanrovi,
        IReadOnlyList<Sifarnik> kategorije)
    {
        var vector = new List<double>();
        var memberCount = Math.Max(1, band.Clanovi.Count(x => x.Aktivan));
        vector.AddRange(Bucket(memberCount, 5, 9));

        var historicalHalls = band.Rezervacije.Select(x => x.Sala).ToArray();
        if (historicalHalls.Length == 0)
        {
            vector.AddRange([0.5, 0.5, 0.5]);
        }
        else
        {
            var averagePrice = historicalHalls.Average(x => (double)x.CijenaPoSatu);
            vector.AddRange(Bucket(averagePrice, 20, 35));
        }

        vector.AddRange(zanrovi.Select(x => x.Id == band.ZanrId ? 1d : 0d));
        var instrumentCodes = band.Clanovi
            .Where(x => x.Aktivan && x.Instrument != null)
            .Select(x => x.Instrument!.Kod)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        vector.AddRange(kategorije.Select(category =>
            EquipmentNeed(category.Kod, instrumentCodes)));

        if (historicalHalls.Length == 0)
        {
            var candidateAcoustics = candidateHalls.Select(AcousticFeatures).ToArray();
            vector.AddRange(candidateAcoustics.Length == 0
                ? [0d, 0d, 0d]
                : Enumerable.Range(0, 3)
                    .Select(index => candidateAcoustics.Average(x => x[index])));
        }
        else
        {
            var historicalAcoustics = historicalHalls.Select(AcousticFeatures).ToArray();
            vector.AddRange(Enumerable.Range(0, 3)
                .Select(index => historicalAcoustics.Average(x => x[index])));
        }
        return [.. vector];
    }

    private static double EquipmentNeed(
        string categoryCode,
        HashSet<string> instruments) =>
        categoryCode switch
        {
            "MIKROFON" => instruments.Contains("VOKAL") ? 1 : 0.25,
            "POJACALO" => instruments.Contains("GITARA") || instruments.Contains("BAS") ? 1 : 0.25,
            "INSTRUMENT" => instruments.Contains("BUBNJEVI") || instruments.Contains("KLAVIJATURE") ? 1 : 0.4,
            "KABLOVI" => instruments.Contains("GITARA")
                || instruments.Contains("BAS")
                || instruments.Contains("KLAVIJATURE") ? 0.9 : 0.3,
            "DODACI" => 0.5,
            _ => 0
        };

    private static double[] AcousticFeatures(Sala sala)
    {
        var text = $"{sala.Akustika} {sala.Opis}".ToUpperInvariant();
        return
        [
            ContainsAny(text, "TRETMAN", "TRETIR", "AKUST") ? 1 : 0,
            ContainsAny(text, "IZOL", "TIH", "BUK") ? 1 : 0,
            ContainsAny(text, "LIVE", "PRIROD", "AMBIJENT", "PROSTOR") ? 1 : 0
        ];
    }

    private static double[] Bucket(double value, double firstLimit, double secondLimit) =>
        value <= firstLimit ? [1, 0, 0]
        : value <= secondLimit ? [0, 1, 0]
        : [0, 0, 1];

    private static bool ContainsAny(string text, params string[] values) =>
        values.Any(text.Contains);

    private static SalaCardDto UCardDto(Sala sala)
    {
        var recenzije = sala.Recenzije.Where(x => x.Vidljiva).ToArray();
        return new SalaCardDto(
            sala.Id,
            sala.Naziv,
            sala.Studio.Naziv,
            sala.Studio.Grad,
            sala.Kapacitet,
            sala.CijenaPoSatu,
            sala.Status.Naziv,
            sala.Galerija.OrderBy(x => x.Redoslijed).Select(x => x.Url).FirstOrDefault(),
            recenzije.Length == 0 ? 0 : decimal.Round((decimal)recenzije.Average(x => x.Ocjena), 1),
            recenzije.Length,
            sala.Oprema.Where(x => x.Status.Kod == "DOSTUPNA")
                .OrderBy(x => x.Naziv).Select(x => x.Naziv).Take(5).ToArray(),
            true);
    }

    private static string Objasni(RecommendationScore score, int historyCount) =>
        historyCount == 0
            ? "Poklapanje žanra, kapaciteta i potrebne opreme"
            : score.CollaborativeScore > score.ContentScore
                ? "Slični bendovi često biraju ovu salu"
                : "Odgovara profilu benda i dosadašnjim rezervacijama";

    private static double Zaokruzi(double value) => Math.Round(value, 4);

    private static void Validiraj(RecommendationFilter filter)
    {
        if (filter.BendId <= 0)
        {
            throw new ArgumentException("Bend je obavezan.");
        }
        if (filter.TerminOdUtc.HasValue != filter.TerminDoUtc.HasValue
            || filter.TerminOdUtc.HasValue && filter.TerminDoUtc <= filter.TerminOdUtc)
        {
            throw new ArgumentException("Termin preporuke nije ispravan.");
        }
    }

    private sealed record Sifarnik(int Id, string Kod);
}
