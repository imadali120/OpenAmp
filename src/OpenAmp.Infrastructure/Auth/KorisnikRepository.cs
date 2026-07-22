using Microsoft.EntityFrameworkCore;
using OpenAmp.Application.Auth;
using OpenAmp.Domain.Entities;
using OpenAmp.Infrastructure.Persistence;

namespace OpenAmp.Infrastructure.Auth;

public sealed class KorisnikRepository(OpenAmpDbContext dbContext) : IKorisnikRepository
{
    public Task<bool> EmailPostojiAsync(string email, CancellationToken cancellationToken = default) =>
        dbContext.Korisnici.AnyAsync(x => x.Email == email, cancellationToken);

    public Task<Korisnik?> DohvatiPoEmailuAsync(string email, CancellationToken cancellationToken = default) =>
        dbContext.Korisnici
            .Include(x => x.Uloga)
            .Include(x => x.RefreshTokeni.Where(t => t.OpozvanUtc == null))
            .SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<Korisnik?> DohvatiPoIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Korisnici
            .Include(x => x.Uloga)
            .Include(x => x.Instrumenti)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Korisnik?> DohvatiPoRefreshTokenHashuAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        dbContext.Korisnici
            .Include(x => x.Uloga)
            .Include(x => x.RefreshTokeni)
            .SingleOrDefaultAsync(x => x.RefreshTokeni.Any(t => t.TokenHash == tokenHash), cancellationToken);

    public async Task<Uloga> DohvatiUloguMuzicaraAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Uloge.SingleAsync(x => x.Kod == "MUZICAR", cancellationToken);

    public async Task<IReadOnlyCollection<Instrument>> DohvatiInstrumenteAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default) =>
        await dbContext.Instrumenti.Where(x => ids.Contains(x.Id)).ToArrayAsync(cancellationToken);

    public async Task DodajAsync(Korisnik korisnik, CancellationToken cancellationToken = default) =>
        await dbContext.Korisnici.AddAsync(korisnik, cancellationToken);
}
