namespace OpenAmp.Domain.Entities;

public sealed class Studio
{
    public int Id { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public string? Opis { get; set; }
    public string Adresa { get; set; } = string.Empty;
    public string Grad { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public bool Aktivan { get; set; } = true;
    public string VremenskaZona { get; set; } = "Europe/Sarajevo";
    public TimeOnly RadnoVrijemeOd { get; set; } = new(8, 0);
    public TimeOnly RadnoVrijemeDo { get; set; } = new(23, 0);
    public int PuniPovratDoSati { get; set; } = 24;
    public int DjelimicniPovratDoSati { get; set; } = 12;
    public int DjelimicniPovratPostotak { get; set; } = 50;
    public int? VlasnikId { get; set; }
    public Korisnik? Vlasnik { get; set; }
    public ICollection<Sala> Sale { get; set; } = [];
    public ICollection<Artikal> Artikli { get; set; } = [];
}

public sealed class Sala
{
    public int Id { get; set; }
    public int StudioId { get; set; }
    public Studio Studio { get; set; } = null!;
    public string Naziv { get; set; } = string.Empty;
    public int Kapacitet { get; set; }
    public decimal CijenaPoSatu { get; set; }
    public int StatusSaleId { get; set; }
    public StatusSale Status { get; set; } = null!;
    public string? Opis { get; set; }
    public string? Akustika { get; set; }
    public decimal? GeografskaSirina { get; set; }
    public decimal? GeografskaDuzina { get; set; }
    public ICollection<SalaSlika> Galerija { get; set; } = [];
    public ICollection<Oprema> Oprema { get; set; } = [];
    public ICollection<Rezervacija> Rezervacije { get; set; } = [];
    public ICollection<Recenzija> Recenzije { get; set; } = [];
}

public sealed class SalaSlika
{
    public int Id { get; set; }
    public int SalaId { get; set; }
    public Sala Sala { get; set; } = null!;
    public string Url { get; set; } = string.Empty;
    public string? AlternativniTekst { get; set; }
    public int Redoslijed { get; set; }
}

public sealed class Oprema
{
    public int Id { get; set; }
    public string InventarskiBroj { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public string? Opis { get; set; }
    public string? SerijskiBroj { get; set; }
    public decimal CijenaNajmaPoSatu { get; set; }
    public DateOnly? DatumNabavke { get; set; }
    public DateOnly? DatumZadnjegServisa { get; set; }
    public string? Napomena { get; set; }
    public int KategorijaOpremeId { get; set; }
    public KategorijaOpreme Kategorija { get; set; } = null!;
    public int StatusOpremeId { get; set; }
    public StatusOpreme Status { get; set; } = null!;
    public int? SalaId { get; set; }
    public Sala? Sala { get; set; }
    public ICollection<StavkaRezervacije> StavkeRezervacija { get; set; } = [];
}

public sealed class Artikal
{
    public int Id { get; set; }
    public string InventarskiBroj { get; set; } = string.Empty;
    public string Naziv { get; set; } = string.Empty;
    public string? Opis { get; set; }
    public int KolicinaNaStanju { get; set; }
    public int MinimalnaZaliha { get; set; }
    public decimal CijenaKupovine { get; set; }
    public int KategorijaArtiklaId { get; set; }
    public KategorijaArtikla Kategorija { get; set; } = null!;
    public int StatusArtiklaId { get; set; }
    public StatusArtikla Status { get; set; } = null!;
    public int StudioId { get; set; }
    public Studio Studio { get; set; } = null!;
    public ICollection<StavkaRezervacije> StavkeRezervacija { get; set; } = [];
}
