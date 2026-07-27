namespace OpenAmp.Domain.Entities;

public interface ISifarnik
{
    int Id { get; set; }
    string Kod { get; set; }
    string Naziv { get; set; }
}

public sealed class Uloga : ISifarnik
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Korisnik> Korisnici { get; set; } = [];
}

public sealed class Zanr : ISifarnik
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Bend> Bendovi { get; set; } = [];
}

public sealed class Instrument : ISifarnik
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<KorisnikInstrument> Korisnici { get; set; } = [];
    public ICollection<ClanBenda> ClanoviBendova { get; set; } = [];
}

public sealed class StatusSale : ISifarnik
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Sala> Sale { get; set; } = [];
}

public sealed class KategorijaOpreme : ISifarnik
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Oprema> Oprema { get; set; } = [];
}

public sealed class StatusOpreme : ISifarnik
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Oprema> Oprema { get; set; } = [];
}

public sealed class KategorijaArtikla : ISifarnik
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Artikal> Artikli { get; set; } = [];
}

public sealed class StatusArtikla : ISifarnik
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Artikal> Artikli { get; set; } = [];
}

public sealed class StatusRezervacije : ISifarnik
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<Rezervacija> Rezervacije { get; set; } = [];
}

public sealed class StatusPozivnice : ISifarnik
{
    public int Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public ICollection<PozivnicaBenda> Pozivnice { get; set; } = [];
}
