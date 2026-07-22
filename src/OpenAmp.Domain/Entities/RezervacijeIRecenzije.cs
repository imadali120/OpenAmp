namespace OpenAmp.Domain.Entities;

public sealed class Rezervacija
{
    public int Id { get; set; }
    public int SalaId { get; set; }
    public Sala Sala { get; set; } = null!;
    public int BendId { get; set; }
    public Bend Bend { get; set; } = null!;
    public int KreiraoKorisnikId { get; set; }
    public Korisnik KreiraoKorisnik { get; set; } = null!;
    public DateTime TerminOdUtc { get; set; }
    public DateTime TerminDoUtc { get; set; }
    public decimal UkupnaCijena { get; set; }
    public int StatusRezervacijeId { get; set; }
    public StatusRezervacije Status { get; set; } = null!;
    public string? Napomena { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeRefundId { get; set; }
    public decimal RefundiraniIznos { get; set; }
    public DateTime? RefundiranUtc { get; set; }
    public DateTime? OtkazanaUtc { get; set; }
    public string? RazlogOtkazivanja { get; set; }
    public DateTime KreiranaUtc { get; set; }
    public DateTime AzuriranaUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<StavkaRezervacije> Stavke { get; set; } = [];
    public Recenzija? Recenzija { get; set; }
}

public sealed class StripeWebhookDogadjaj
{
    public string Id { get; set; } = string.Empty;
    public string Tip { get; set; } = string.Empty;
    public DateTime ObradjenUtc { get; set; }
}

public sealed class StavkaRezervacije
{
    public int Id { get; set; }
    public int RezervacijaId { get; set; }
    public Rezervacija Rezervacija { get; set; } = null!;
    public int? OpremaId { get; set; }
    public Oprema? Oprema { get; set; }
    public int? ArtikalId { get; set; }
    public Artikal? Artikal { get; set; }
    public string Naziv { get; set; } = string.Empty;
    public int Kolicina { get; set; }
    public decimal JedinicnaCijena { get; set; }
    public decimal BrojSati { get; set; }
    public decimal UkupnaCijena { get; set; }
}

public sealed class Recenzija
{
    public int Id { get; set; }
    public int Ocjena { get; set; }
    public string? Komentar { get; set; }
    public DateTime KreiranaUtc { get; set; }
    public bool Vidljiva { get; set; } = true;
    public int KorisnikId { get; set; }
    public Korisnik Korisnik { get; set; } = null!;
    public int SalaId { get; set; }
    public Sala Sala { get; set; } = null!;
    public int? RezervacijaId { get; set; }
    public Rezervacija? Rezervacija { get; set; }
}
