using OpenAmp.Application.Common;

namespace OpenAmp.Application.Mobile;

public sealed record SifarnikDto(int Id, string Kod, string Naziv);

public sealed record MobileLookupsDto(
    IReadOnlyCollection<SifarnikDto> Zanrovi,
    IReadOnlyCollection<SifarnikDto> KategorijeOpreme,
    IReadOnlyCollection<SifarnikDto> Instrumenti);

public sealed record SalaCardDto(
    int Id,
    string Naziv,
    string Studio,
    string Grad,
    int Kapacitet,
    decimal CijenaPoSatu,
    string Status,
    string? SlikaUrl,
    decimal ProsjecnaOcjena,
    int BrojRecenzija,
    IReadOnlyCollection<string> Oprema,
    bool Dostupna);

public sealed record OpremaZaNajamDto(
    int Id,
    string Naziv,
    string Kategorija,
    string? Opis,
    decimal CijenaPoSatu,
    bool Dostupna);

public sealed record ArtikalZaKupovinuDto(
    int Id,
    string Naziv,
    string Kategorija,
    string? Opis,
    decimal Cijena,
    int NaStanju);

public sealed record RecenzijaSaleDto(
    int Id,
    int Ocjena,
    string? Komentar,
    string Korisnik,
    DateTime KreiranaUtc);

public sealed record SalaDetaljiDto(
    int Id,
    string Naziv,
    string Studio,
    string Grad,
    string Adresa,
    int Kapacitet,
    decimal CijenaPoSatu,
    string? Opis,
    string? Akustika,
    decimal? GeografskaSirina,
    decimal? GeografskaDuzina,
    decimal ProsjecnaOcjena,
    int BrojRecenzija,
    IReadOnlyCollection<string> Galerija,
    IReadOnlyCollection<OpremaZaNajamDto> Oprema,
    IReadOnlyCollection<ArtikalZaKupovinuDto> Artikli,
    IReadOnlyCollection<RecenzijaSaleDto> Recenzije);

public sealed record ClanBendaDto(
    int KorisnikId,
    string ImePrezime,
    string? Instrument,
    string? Uloga,
    bool Osnivac);

public sealed record PozivnicaBendaDto(
    int Id,
    string Email,
    string Kod,
    string Status,
    DateTime IsticeUtc);

public sealed record PrimljenaPozivnicaBendaDto(
    int Id,
    int BendId,
    string Bend,
    string Zanr,
    string Pozvao,
    string Kod,
    string Status,
    DateTime KreiranaUtc,
    DateTime IsticeUtc);

public sealed record BendDto(
    int Id,
    string Naziv,
    string Zanr,
    string? Opis,
    string? FotografijaUrl,
    bool JeOsnivac,
    int BrojRezervacija,
    IReadOnlyCollection<ClanBendaDto> Clanovi,
    IReadOnlyCollection<PozivnicaBendaDto> Pozivnice);

public sealed record MobileRezervacijaDto(
    int Id,
    int SalaId,
    string Sala,
    string Studio,
    int BendId,
    string Bend,
    DateTime TerminOdUtc,
    DateTime TerminDoUtc,
    decimal UkupnaCijena,
    string Status,
    string StatusKod,
    string RowVersion,
    string? SlikaUrl,
    bool MozeOtkazati,
    bool MozeRecenzirati);

public sealed record KorisnickePostavkeDto(
    bool PushNotifikacije,
    bool EmailNotifikacije,
    string Jezik,
    bool ProfilJavan);

public sealed record ProfilPregledDto(
    int Id,
    string Ime,
    string Prezime,
    string Email,
    string? Telefon,
    string? FotografijaUrl,
    IReadOnlyCollection<string> Instrumenti,
    int BrojBendova,
    int BrojRezervacija,
    decimal UkupnoSati,
    int BrojRecenzija,
    string? OmiljenaSala,
    string? NajcesciZanr);

public sealed record PretraziSaleQuery(
    string? Tekst,
    string? ZanrKod,
    int? MinimalniKapacitet,
    string? KategorijaOpremeKod,
    DateTime? TerminOdUtc,
    DateTime? TerminDoUtc) : IQuery<IReadOnlyCollection<SalaCardDto>>;

public sealed record DohvatiSaluQuery(int SalaId) : IQuery<SalaDetaljiDto>;
public sealed record DohvatiMobileSifarnikeQuery : IQuery<MobileLookupsDto>;
public sealed record DohvatiMojeBendoveQuery(int KorisnikId) : IQuery<IReadOnlyCollection<BendDto>>;
public sealed record DohvatiMojeRezervacijeQuery(int KorisnikId) : IQuery<IReadOnlyCollection<MobileRezervacijaDto>>;
public sealed record DohvatiProfilPregledQuery(int KorisnikId) : IQuery<ProfilPregledDto>;
public sealed record DohvatiPrimljenePozivniceQuery(int KorisnikId)
    : IQuery<IReadOnlyCollection<PrimljenaPozivnicaBendaDto>>;
public sealed record DohvatiOmiljeneSaleQuery(int KorisnikId) : IQuery<IReadOnlyCollection<int>>;
public sealed record DohvatiKorisnickePostavkeQuery(int KorisnikId) : IQuery<KorisnickePostavkeDto>;

public sealed record KreirajBendCommand(
    int KorisnikId,
    string Naziv,
    int ZanrId,
    string? Opis) : ICommand<BendDto>;

public sealed record PosaljiPozivnicuBendaCommand(
    int KorisnikId,
    int BendId,
    string Email) : ICommand<PozivnicaBendaDto>;

public sealed record OdgovoriNaPozivnicuBendaCommand(
    int KorisnikId,
    int PozivnicaId,
    bool Prihvati,
    int? InstrumentId) : ICommand<PrimljenaPozivnicaBendaDto>;

public sealed record AzurirajBendCommand(
    int KorisnikId,
    int BendId,
    string Naziv,
    int ZanrId,
    string? Opis) : ICommand<BendDto>;

public sealed record AzurirajClanaBendaCommand(
    int KorisnikId,
    int BendId,
    int ClanKorisnikId,
    int? InstrumentId,
    string? Uloga) : ICommand<BendDto>;

public sealed record UkloniClanaBendaCommand(
    int KorisnikId,
    int BendId,
    int ClanKorisnikId) : ICommand<BendDto>;

public sealed record PostaviOmiljenuSaluCommand(int KorisnikId, int SalaId, bool Sacuvana) : ICommand<bool>;

public sealed record AzurirajKorisnickePostavkeCommand(
    int KorisnikId,
    bool PushNotifikacije,
    bool EmailNotifikacije,
    string Jezik,
    bool ProfilJavan) : ICommand<KorisnickePostavkeDto>;

public sealed record KreirajRecenzijuCommand(
    int KorisnikId,
    int RezervacijaId,
    int Ocjena,
    string? Komentar) : ICommand<RecenzijaSaleDto>;

public interface IMobileExperienceService
{
    Task<IReadOnlyCollection<SalaCardDto>> PretraziSaleAsync(
        PretraziSaleQuery query,
        CancellationToken cancellationToken = default);

    Task<SalaDetaljiDto> DohvatiSaluAsync(int salaId, CancellationToken cancellationToken = default);
    Task<MobileLookupsDto> DohvatiSifarnikeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BendDto>> DohvatiBendoveAsync(int korisnikId, CancellationToken cancellationToken = default);
    Task<BendDto> KreirajBendAsync(KreirajBendCommand command, CancellationToken cancellationToken = default);
    Task<PozivnicaBendaDto> PosaljiPozivnicuAsync(
        PosaljiPozivnicuBendaCommand command,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PrimljenaPozivnicaBendaDto>> DohvatiPrimljenePozivniceAsync(
        int korisnikId,
        CancellationToken cancellationToken = default);
    Task<PrimljenaPozivnicaBendaDto> OdgovoriNaPozivnicuAsync(
        OdgovoriNaPozivnicuBendaCommand command,
        CancellationToken cancellationToken = default);
    Task<BendDto> AzurirajBendAsync(AzurirajBendCommand command, CancellationToken cancellationToken = default);
    Task<BendDto> AzurirajClanaAsync(AzurirajClanaBendaCommand command, CancellationToken cancellationToken = default);
    Task<BendDto> UkloniClanaAsync(UkloniClanaBendaCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MobileRezervacijaDto>> DohvatiRezervacijeAsync(
        int korisnikId,
        CancellationToken cancellationToken = default);
    Task<ProfilPregledDto> DohvatiProfilAsync(int korisnikId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<int>> DohvatiOmiljeneSaleAsync(
        int korisnikId,
        CancellationToken cancellationToken = default);
    Task<bool> PostaviOmiljenuSaluAsync(
        PostaviOmiljenuSaluCommand command,
        CancellationToken cancellationToken = default);
    Task<KorisnickePostavkeDto> DohvatiPostavkeAsync(
        int korisnikId,
        CancellationToken cancellationToken = default);
    Task<KorisnickePostavkeDto> AzurirajPostavkeAsync(
        AzurirajKorisnickePostavkeCommand command,
        CancellationToken cancellationToken = default);
    Task<RecenzijaSaleDto> KreirajRecenzijuAsync(
        KreirajRecenzijuCommand command,
        CancellationToken cancellationToken = default);
}
