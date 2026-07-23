using OpenAmp.Application.Common;
using OpenAmp.Domain.Entities;

namespace OpenAmp.Application.Auth;

public sealed class RegisterCommandHandler(
    IKorisnikRepository korisnikRepository,
    IPasswordHasher passwordHasher,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<RegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> HandleAsync(
        RegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizujEmail(command.Email);
        var username = CredentialPolicy.NormalizeUsername(command.Username);
        CredentialPolicy.ValidatePassword(command.Password);
        if (await korisnikRepository.EmailPostojiAsync(email, cancellationToken))
        {
            throw new EmailJeZauzetException();
        }
        if (await korisnikRepository.UsernamePostojiAsync(username, cancellationToken: cancellationToken))
        {
            throw new UsernameJeZauzetException();
        }

        var sadaUtc = timeProvider.GetUtcNow().UtcDateTime;
        var uloga = await korisnikRepository.DohvatiUloguMuzicaraAsync(cancellationToken);
        var korisnik = new Korisnik
        {
            Username = username,
            Ime = command.Ime.Trim(),
            Prezime = command.Prezime.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(command.Password),
            Telefon = command.Telefon?.Trim(),
            Aktivan = true,
            KreiranUtc = sadaUtc,
            UlogaId = uloga.Id,
            Uloga = uloga
        };

        var refresh = refreshTokenService.Kreiraj(sadaUtc);
        korisnik.RefreshTokeni.Add(KreirajRefreshEntitet(refresh, command.IpAdresa, sadaUtc));
        await korisnikRepository.DodajAsync(korisnik, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return KreirajOdgovor(korisnik, accessTokenService.Kreiraj(korisnik), refresh);
    }

    internal static string NormalizujEmail(string email) => email.Trim().ToLowerInvariant();

    internal static RefreshToken KreirajRefreshEntitet(
        GenerisaniRefreshToken refresh,
        string? ipAdresa,
        DateTime sadaUtc) => new()
        {
            TokenHash = refresh.Hash,
            KreiranUtc = sadaUtc,
            IsticeUtc = refresh.IsticeUtc,
            KreiranSaIpAdrese = ipAdresa
        };

    internal static AuthResponseDto KreirajOdgovor(
        Korisnik korisnik,
        AccessTokenResult access,
        GenerisaniRefreshToken refresh) => new(
            access.Token,
            access.IsticeUtc,
            refresh.Vrijednost,
            refresh.IsticeUtc,
            new KorisnikDto(
                korisnik.Id,
                korisnik.Username,
                korisnik.Ime,
                korisnik.Prezime,
                korisnik.Email,
                korisnik.Telefon,
                korisnik.Uloga.Naziv,
                korisnik.FotografijaUrl,
                korisnik.Instrumenti.Select(x => x.InstrumentId).ToArray()));
}

public sealed class LoginCommandHandler(
    IKorisnikRepository korisnikRepository,
    IPasswordHasher passwordHasher,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var korisnik = await korisnikRepository.DohvatiPoEmailuIliUsernameuAsync(
            command.Email.Trim().ToLowerInvariant(),
            cancellationToken);

        if (korisnik is null || !korisnik.Aktivan || !passwordHasher.Verify(command.Password, korisnik.PasswordHash))
        {
            throw new NeispravniPodaciZaPrijavuException();
        }

        var sadaUtc = timeProvider.GetUtcNow().UtcDateTime;
        var refresh = refreshTokenService.Kreiraj(sadaUtc);
        korisnik.RefreshTokeni.Add(RegisterCommandHandler.KreirajRefreshEntitet(refresh, command.IpAdresa, sadaUtc));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RegisterCommandHandler.KreirajOdgovor(
            korisnik,
            accessTokenService.Kreiraj(korisnik),
            refresh);
    }
}

public sealed class RefreshTokenCommandHandler(
    IKorisnikRepository korisnikRepository,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var sadaUtc = timeProvider.GetUtcNow().UtcDateTime;
        var stariHash = refreshTokenService.Hash(command.RefreshToken);
        var korisnik = await korisnikRepository.DohvatiPoRefreshTokenHashuAsync(stariHash, cancellationToken)
            ?? throw new NeispravanRefreshTokenException();
        var stariToken = korisnik.RefreshTokeni.Single(x => x.TokenHash == stariHash);

        if (!stariToken.Aktivan(sadaUtc) || !korisnik.Aktivan)
        {
            throw new NeispravanRefreshTokenException();
        }

        var novi = refreshTokenService.Kreiraj(sadaUtc);
        stariToken.OpozvanUtc = sadaUtc;
        stariToken.ZamijenjenTokenHash = novi.Hash;
        korisnik.RefreshTokeni.Add(RegisterCommandHandler.KreirajRefreshEntitet(novi, command.IpAdresa, sadaUtc));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RegisterCommandHandler.KreirajOdgovor(
            korisnik,
            accessTokenService.Kreiraj(korisnik),
            novi);
    }
}

public sealed class DohvatiKorisnikaQueryHandler(IKorisnikRepository korisnikRepository)
    : IQueryHandler<DohvatiKorisnikaQuery, KorisnikDto>
{
    public async Task<KorisnikDto> HandleAsync(
        DohvatiKorisnikaQuery query,
        CancellationToken cancellationToken = default)
    {
        var korisnik = await korisnikRepository.DohvatiPoIdAsync(query.KorisnikId, cancellationToken)
            ?? throw new KorisnikNijePronadjenException();
        return Mapiraj(korisnik);
    }

    internal static KorisnikDto Mapiraj(Korisnik korisnik) => new(
        korisnik.Id,
        korisnik.Username,
        korisnik.Ime,
        korisnik.Prezime,
        korisnik.Email,
        korisnik.Telefon,
        korisnik.Uloga.Naziv,
        korisnik.FotografijaUrl,
        korisnik.Instrumenti.Select(x => x.InstrumentId).ToArray());
}

public sealed class AzurirajProfilCommandHandler(
    IKorisnikRepository korisnikRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AzurirajProfilCommand, KorisnikDto>
{
    public async Task<KorisnikDto> HandleAsync(
        AzurirajProfilCommand command,
        CancellationToken cancellationToken = default)
    {
        var korisnik = await korisnikRepository.DohvatiPoIdAsync(command.KorisnikId, cancellationToken)
            ?? throw new KorisnikNijePronadjenException();
        var username = CredentialPolicy.NormalizeUsername(command.Username);
        if (await korisnikRepository.UsernamePostojiAsync(
                username,
                korisnik.Id,
                cancellationToken))
        {
            throw new UsernameJeZauzetException();
        }
        korisnik.Username = username;
        korisnik.Ime = command.Ime.Trim();
        korisnik.Prezime = command.Prezime.Trim();
        korisnik.Telefon = command.Telefon?.Trim();
        if (!string.IsNullOrWhiteSpace(command.FotografijaUrl))
        {
            korisnik.FotografijaUrl = command.FotografijaUrl.Trim();
        }

        var instrumentIds = command.InstrumentIds.Distinct().ToArray();
        var instrumenti = await korisnikRepository.DohvatiInstrumenteAsync(instrumentIds, cancellationToken);
        if (instrumenti.Count != instrumentIds.Length)
        {
            throw new ArgumentException("Jedan ili više odabranih instrumenata ne postoje.");
        }

        korisnik.Instrumenti.Clear();
        foreach (var instrument in instrumenti)
        {
            korisnik.Instrumenti.Add(new KorisnikInstrument
            {
                KorisnikId = korisnik.Id,
                InstrumentId = instrument.Id,
                Instrument = instrument,
                Primarni = instrument.Id == instrumentIds.FirstOrDefault()
            });
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return DohvatiKorisnikaQueryHandler.Mapiraj(korisnik);
    }
}

public sealed class PromijeniLozinkuCommandHandler(
    IKorisnikRepository korisnikRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : ICommandHandler<PromijeniLozinkuCommand, bool>
{
    public async Task<bool> HandleAsync(
        PromijeniLozinkuCommand command,
        CancellationToken cancellationToken = default)
    {
        var korisnik = await korisnikRepository.DohvatiPoIdAsync(command.KorisnikId, cancellationToken)
            ?? throw new KorisnikNijePronadjenException();
        if (!passwordHasher.Verify(command.TrenutnaLozinka, korisnik.PasswordHash))
        {
            throw new NeispravniPodaciZaPrijavuException();
        }

        CredentialPolicy.ValidatePassword(command.NovaLozinka);
        korisnik.PasswordHash = passwordHasher.Hash(command.NovaLozinka);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
