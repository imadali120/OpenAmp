namespace OpenAmp.Domain.Entities;

public sealed class Uloga
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Korisnik> Korisnici { get; set; } = [];
}

public sealed class Zanr
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Bend> Bendovi { get; set; } = [];
}

public sealed class Instrument
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<KorisnikInstrument> Korisnici { get; set; } = [];
    public ICollection<ClanBenda> ClanoviBendova { get; set; } = [];
}

public sealed class StatusSale
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Sala> Sale { get; set; } = [];
}

public sealed class KategorijaOpreme
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Oprema> Oprema { get; set; } = [];
}

public sealed class StatusOpreme
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Oprema> Oprema { get; set; } = [];
}

public sealed class KategorijaArtikla
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Artikal> Artikli { get; set; } = [];
}

public sealed class StatusArtikla
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Artikal> Artikli { get; set; } = [];
}

public sealed class StatusRezervacije
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Rezervacija> Rezervacije { get; set; } = [];
}

public sealed class StatusPozivnice
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<PozivnicaBenda> Pozivnice { get; set; } = [];
}
