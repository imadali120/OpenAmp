namespace OpenAmp.Application.Desktop;

public sealed record DesktopSifarnikDto(int Id, string Kod, string Naziv);

public sealed record DesktopLookupsDto(
    IReadOnlyCollection<DesktopSifarnikDto> Studiji,
    IReadOnlyCollection<DesktopSifarnikDto> StatusiSala,
    IReadOnlyCollection<DesktopSifarnikDto> KategorijeOpreme,
    IReadOnlyCollection<DesktopSifarnikDto> StatusiOpreme,
    IReadOnlyCollection<DesktopSifarnikDto> KategorijeArtikala,
    IReadOnlyCollection<DesktopSifarnikDto> StatusiArtikala,
    IReadOnlyCollection<DesktopSifarnikDto> StatusiRezervacija,
    IReadOnlyCollection<DesktopSifarnikDto> Zanrovi,
    IReadOnlyCollection<DesktopSifarnikDto> Uloge);

public sealed record DashboardRezervacijaDto(
    int Id,
    string Vrijeme,
    string Bend,
    string Sala,
    string Zanr,
    string Status);

public sealed record ZauzetostSaleDto(
    int SalaId,
    string Sala,
    DateOnly Datum,
    int Postotak);

public sealed record DesktopDashboardDto(
    int DanasnjeProbe,
    int AktivneSale,
    int OpremaNaNajmu,
    int NiskeZalihe,
    IReadOnlyCollection<DashboardRezervacijaDto> RasporedDanas,
    IReadOnlyCollection<ZauzetostSaleDto> ZauzetostSedmica,
    IReadOnlyCollection<DesktopArtikalDto> UpozorenjaZaliha);

public sealed record DesktopSalaDto(
    int Id,
    int StudioId,
    string Studio,
    string Naziv,
    int Kapacitet,
    decimal CijenaPoSatu,
    int StatusId,
    string Status,
    string StatusKod,
    string? Opis,
    string? Akustika,
    string? SlikaUrl,
    IReadOnlyCollection<string> Oprema);

public sealed record SacuvajSaluDto(
    int StudioId,
    string Naziv,
    int Kapacitet,
    decimal CijenaPoSatu,
    int StatusId,
    string? Opis,
    string? Akustika);

public sealed record ServisOpremeDto(
    int Id,
    DateTime PrijavljenUtc,
    DateTime? ZavrsenUtc,
    string OpisKvara,
    string? IzvrseniRadovi,
    decimal Trosak,
    string Prijavio);

public sealed record DesktopOpremaDto(
    int Id,
    string InventarskiBroj,
    string Naziv,
    string? Opis,
    string? SerijskiBroj,
    decimal CijenaNajmaPoSatu,
    int Stanje,
    DateOnly? DatumNabavke,
    DateOnly? DatumZadnjegServisa,
    string? Napomena,
    int KategorijaId,
    string Kategorija,
    int StatusId,
    string Status,
    string StatusKod,
    int? SalaId,
    string? Sala,
    IReadOnlyCollection<ServisOpremeDto> ServisnaHistorija);

public sealed record SacuvajOpremuDto(
    string InventarskiBroj,
    string Naziv,
    string? Opis,
    string? SerijskiBroj,
    decimal CijenaNajmaPoSatu,
    int Stanje,
    DateOnly? DatumNabavke,
    string? Napomena,
    int KategorijaId,
    int StatusId,
    int? SalaId);

public sealed record PrijaviServisDto(string OpisKvara);

public sealed record ZavrsiServisDto(
    string IzvrseniRadovi,
    decimal Trosak,
    int Stanje,
    int StatusId);

public sealed record DesktopArtikalDto(
    int Id,
    string InventarskiBroj,
    string Naziv,
    string? Opis,
    int KolicinaNaStanju,
    int MinimalnaZaliha,
    decimal Cijena,
    int KategorijaId,
    string Kategorija,
    int StatusId,
    string Status,
    int StudioId,
    string Studio,
    bool NiskaZaliha);

public sealed record SacuvajArtikalDto(
    string InventarskiBroj,
    string Naziv,
    string? Opis,
    int KolicinaNaStanju,
    int MinimalnaZaliha,
    decimal Cijena,
    int KategorijaId,
    int StatusId,
    int StudioId);

public sealed record DesktopRezervacijaDto(
    int Id,
    int SalaId,
    string Sala,
    int BendId,
    string Bend,
    string Zanr,
    DateTime TerminOdUtc,
    DateTime TerminDoUtc,
    decimal UkupnaCijena,
    int StatusId,
    string Status,
    string StatusKod,
    string? Napomena,
    string RowVersion);

public sealed record SacuvajDesktopRezervacijuDto(
    int SalaId,
    int BendId,
    DateTime TerminOdUtc,
    DateTime TerminDoUtc,
    string? Napomena);

public sealed record IzmijeniDesktopRezervacijuDto(
    int SalaId,
    DateTime TerminOdUtc,
    DateTime TerminDoUtc,
    int StatusId,
    string? Napomena,
    string RowVersion);

public sealed record DesktopClanBendaDto(
    int KorisnikId,
    string Username,
    string ImePrezime,
    string? Instrument,
    string? Uloga,
    bool Osnivac);

public sealed record DesktopBendDto(
    int Id,
    string Naziv,
    int ZanrId,
    string Zanr,
    string? Opis,
    string? SlikaUrl,
    int BrojRezervacija,
    IReadOnlyCollection<DesktopClanBendaDto> Clanovi);

public sealed record IzmijeniDesktopBendDto(string Naziv, int ZanrId, string? Opis);

public sealed record DesktopKorisnikDto(
    int Id,
    string Username,
    string Ime,
    string Prezime,
    string Email,
    string? Telefon,
    int UlogaId,
    string Uloga,
    string UlogaKod,
    bool Aktivan,
    DateTime KreiranUtc);

public sealed record IzmijeniDesktopKorisnikDto(int UlogaId, bool Aktivan);

public interface IDesktopAdminService
{
    Task<DesktopLookupsDto> DohvatiSifarnikeAsync(CancellationToken cancellationToken = default);
    Task<DesktopDashboardDto> DohvatiDashboardAsync(int? studioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DesktopSalaDto>> DohvatiSaleAsync(string? tekst, int? statusId, int? minimalniKapacitet, CancellationToken cancellationToken = default);
    Task<DesktopSalaDto> SacuvajSaluAsync(int? id, SacuvajSaluDto dto, CancellationToken cancellationToken = default);
    Task ObrisiSaluAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DesktopOpremaDto>> DohvatiOpremuAsync(int? kategorijaId, int? statusId, int? salaId, CancellationToken cancellationToken = default);
    Task<DesktopOpremaDto> SacuvajOpremuAsync(int? id, SacuvajOpremuDto dto, CancellationToken cancellationToken = default);
    Task<DesktopOpremaDto> PrijaviServisAsync(int id, int korisnikId, PrijaviServisDto dto, CancellationToken cancellationToken = default);
    Task<DesktopOpremaDto> ZavrsiServisAsync(int id, int servisId, ZavrsiServisDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DesktopArtikalDto>> DohvatiArtikleAsync(int? studioId, bool samoNiskaZaliha, CancellationToken cancellationToken = default);
    Task<DesktopArtikalDto> SacuvajArtikalAsync(int? id, SacuvajArtikalDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DesktopRezervacijaDto>> DohvatiRezervacijeAsync(DateTime odUtc, DateTime doUtc, int? salaId, CancellationToken cancellationToken = default);
    Task<DesktopRezervacijaDto> KreirajRezervacijuAsync(int korisnikId, SacuvajDesktopRezervacijuDto dto, CancellationToken cancellationToken = default);
    Task<DesktopRezervacijaDto> IzmijeniRezervacijuAsync(int id, IzmijeniDesktopRezervacijuDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DesktopBendDto>> DohvatiBendoveAsync(string? tekst, int? zanrId, CancellationToken cancellationToken = default);
    Task<DesktopBendDto> IzmijeniBendAsync(int id, IzmijeniDesktopBendDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DesktopKorisnikDto>> DohvatiKorisnikeAsync(string? tekst, CancellationToken cancellationToken = default);
    Task<DesktopKorisnikDto> IzmijeniKorisnikaAsync(int id, IzmijeniDesktopKorisnikDto dto, CancellationToken cancellationToken = default);
}
