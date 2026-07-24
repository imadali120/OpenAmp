namespace OpenAmp.Desktop.Models;

public sealed class AuthSession
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public UserIdentity Korisnik { get; set; } = new();
}

public sealed class UserIdentity
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Ime { get; set; } = "";
    public string Prezime { get; set; } = "";
    public string Uloga { get; set; } = "";
}

public sealed class LookupItem
{
    public int Id { get; set; }
    public string Kod { get; set; } = "";
    public string Naziv { get; set; } = "";
    public override string ToString() => Naziv;
}

public sealed class DesktopLookups
{
    public List<LookupItem> Studiji { get; set; } = [];
    public List<LookupItem> StatusiSala { get; set; } = [];
    public List<LookupItem> KategorijeOpreme { get; set; } = [];
    public List<LookupItem> StatusiOpreme { get; set; } = [];
    public List<LookupItem> KategorijeArtikala { get; set; } = [];
    public List<LookupItem> StatusiArtikala { get; set; } = [];
    public List<LookupItem> StatusiRezervacija { get; set; } = [];
    public List<LookupItem> Zanrovi { get; set; } = [];
    public List<LookupItem> Uloge { get; set; } = [];
}

public sealed class DashboardData
{
    public int DanasnjeProbe { get; set; }
    public int AktivneSale { get; set; }
    public int OpremaNaNajmu { get; set; }
    public int NiskeZalihe { get; set; }
    public List<DashboardReservation> RasporedDanas { get; set; } = [];
    public List<OccupancyCell> ZauzetostSedmica { get; set; } = [];
    public List<ArticleItem> UpozorenjaZaliha { get; set; } = [];
}

public sealed class DashboardReservation
{
    public int Id { get; set; }
    public string Vrijeme { get; set; } = "";
    public string Bend { get; set; } = "";
    public string Sala { get; set; } = "";
    public string Zanr { get; set; } = "";
    public string Status { get; set; } = "";
}

public sealed class OccupancyCell
{
    public int SalaId { get; set; }
    public string Sala { get; set; } = "";
    public DateOnly Datum { get; set; }
    public int Postotak { get; set; }
}

public sealed class HallItem
{
    public int Id { get; set; }
    public int StudioId { get; set; }
    public string Studio { get; set; } = "";
    public string Naziv { get; set; } = "";
    public int Kapacitet { get; set; }
    public decimal CijenaPoSatu { get; set; }
    public int StatusId { get; set; }
    public string Status { get; set; } = "";
    public string StatusKod { get; set; } = "";
    public string? Opis { get; set; }
    public string? Akustika { get; set; }
    public string? SlikaUrl { get; set; }
    public List<string> Oprema { get; set; } = [];
    public string OpremaTekst => string.Join("  ·  ", Oprema);
}

public sealed class ServiceItem
{
    public int Id { get; set; }
    public DateTime PrijavljenUtc { get; set; }
    public DateTime? ZavrsenUtc { get; set; }
    public string OpisKvara { get; set; } = "";
    public string? IzvrseniRadovi { get; set; }
    public decimal Trosak { get; set; }
    public string Prijavio { get; set; } = "";
}

public sealed class EquipmentItem
{
    public int Id { get; set; }
    public string InventarskiBroj { get; set; } = "";
    public string Naziv { get; set; } = "";
    public string? Opis { get; set; }
    public string? SerijskiBroj { get; set; }
    public decimal CijenaNajmaPoSatu { get; set; }
    public int Stanje { get; set; }
    public DateOnly? DatumNabavke { get; set; }
    public DateOnly? DatumZadnjegServisa { get; set; }
    public string? Napomena { get; set; }
    public int KategorijaId { get; set; }
    public string Kategorija { get; set; } = "";
    public int StatusId { get; set; }
    public string Status { get; set; } = "";
    public string StatusKod { get; set; } = "";
    public int? SalaId { get; set; }
    public string? Sala { get; set; }
    public List<ServiceItem> ServisnaHistorija { get; set; } = [];
    public string StanjeTekst => new string('●', Stanje) + new string('○', 5 - Stanje);
}

public sealed class ArticleItem
{
    public int Id { get; set; }
    public string InventarskiBroj { get; set; } = "";
    public string Naziv { get; set; } = "";
    public string? Opis { get; set; }
    public int KolicinaNaStanju { get; set; }
    public int MinimalnaZaliha { get; set; }
    public decimal Cijena { get; set; }
    public int KategorijaId { get; set; }
    public string Kategorija { get; set; } = "";
    public int StatusId { get; set; }
    public string Status { get; set; } = "";
    public int StudioId { get; set; }
    public string Studio { get; set; } = "";
    public bool NiskaZaliha { get; set; }
    public string StanjeZalihe => $"{KolicinaNaStanju} / min. {MinimalnaZaliha}";
}

public sealed class ReservationItem
{
    public int Id { get; set; }
    public int SalaId { get; set; }
    public string Sala { get; set; } = "";
    public int BendId { get; set; }
    public string Bend { get; set; } = "";
    public string Zanr { get; set; } = "";
    public DateTime TerminOdUtc { get; set; }
    public DateTime TerminDoUtc { get; set; }
    public decimal UkupnaCijena { get; set; }
    public int StatusId { get; set; }
    public string Status { get; set; } = "";
    public string StatusKod { get; set; } = "";
    public string? Napomena { get; set; }
    public string RowVersion { get; set; } = "";
    public string Vrijeme => $"{TerminOdUtc.ToLocalTime():HH:mm}–{TerminDoUtc.ToLocalTime():HH:mm}";
}

public sealed class BandMember
{
    public int KorisnikId { get; set; }
    public string Username { get; set; } = "";
    public string ImePrezime { get; set; } = "";
    public string? Instrument { get; set; }
    public string? Uloga { get; set; }
    public bool Osnivac { get; set; }
}

public sealed class BandItem
{
    public int Id { get; set; }
    public string Naziv { get; set; } = "";
    public int ZanrId { get; set; }
    public string Zanr { get; set; } = "";
    public string? Opis { get; set; }
    public string? SlikaUrl { get; set; }
    public int BrojRezervacija { get; set; }
    public List<BandMember> Clanovi { get; set; } = [];
    public string Statistika => $"{Clanovi.Count} članova  ·  {BrojRezervacija} rezervacija";
}

public sealed class UserItem
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Ime { get; set; } = "";
    public string Prezime { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Telefon { get; set; }
    public int UlogaId { get; set; }
    public string Uloga { get; set; } = "";
    public string UlogaKod { get; set; } = "";
    public bool Aktivan { get; set; }
    public DateTime KreiranUtc { get; set; }
    public string ImePrezime => $"{Ime} {Prezime}";
}

public sealed class WeekDayColumn
{
    public DateOnly Datum { get; init; }
    public string Dan { get; init; } = "";
    public string DatumTekst { get; init; } = "";
    public List<ReservationItem> Rezervacije { get; init; } = [];
}
