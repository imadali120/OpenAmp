namespace OpenAmp.Domain.Entities;

public sealed class Korisnik
{
    public int Id { get; set; }
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? FotografijaUrl { get; set; }
    public bool Aktivan { get; set; } = true;
    public DateTime KreiranUtc { get; set; }
    public int UlogaId { get; set; }
    public Uloga Uloga { get; set; } = null!;
    public ICollection<KorisnikInstrument> Instrumenti { get; set; } = [];
    public ICollection<ClanBenda> Clanstva { get; set; } = [];
    public ICollection<Bend> OsnovaniBendovi { get; set; } = [];
    public ICollection<PozivnicaBenda> PoslanePozivnice { get; set; } = [];
    public ICollection<PozivnicaBenda> PrimljenePozivnice { get; set; } = [];
    public ICollection<Rezervacija> KreiraneRezervacije { get; set; } = [];
    public ICollection<Recenzija> Recenzije { get; set; } = [];
    public ICollection<Studio> Studiji { get; set; } = [];
}

public sealed class KorisnikInstrument
{
    public int KorisnikId { get; set; }
    public Korisnik Korisnik { get; set; } = null!;
    public int InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    public bool Primarni { get; set; }
}

public sealed class Bend
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string? Opis { get; set; }
    public string? FotografijaUrl { get; set; }
    public DateTime KreiranUtc { get; set; }
    public int OsnivacId { get; set; }
    public Korisnik Osnivac { get; set; } = null!;
    public int ZanrId { get; set; }
    public Zanr Zanr { get; set; } = null!;
    public ICollection<ClanBenda> Clanovi { get; set; } = [];
    public ICollection<PozivnicaBenda> Pozivnice { get; set; } = [];
    public ICollection<Rezervacija> Rezervacije { get; set; } = [];
}

public sealed class ClanBenda
{
    public int BendId { get; set; }
    public Bend Bend { get; set; } = null!;
    public int KorisnikId { get; set; }
    public Korisnik Korisnik { get; set; } = null!;
    public int? InstrumentId { get; set; }
    public Instrument? Instrument { get; set; }
    public string? UlogaUBendu { get; set; }
    public DateTime DatumPridruzivanjaUtc { get; set; }
    public bool Aktivan { get; set; } = true;
}

public sealed class PozivnicaBenda
{
    public int Id { get; set; }
    public int BendId { get; set; }
    public Bend Bend { get; set; } = null!;
    public int PozvaoKorisnikId { get; set; }
    public Korisnik PozvaoKorisnik { get; set; } = null!;
    public int? PozvaniKorisnikId { get; set; }
    public Korisnik? PozvaniKorisnik { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Kod { get; set; } = string.Empty;
    public int StatusPozivniceId { get; set; }
    public StatusPozivnice Status { get; set; } = null!;
    public DateTime KreiranaUtc { get; set; }
    public DateTime IsticeUtc { get; set; }
    public DateTime? OdgovorenaUtc { get; set; }
}
