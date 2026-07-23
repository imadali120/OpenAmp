namespace OpenAmp.Domain.Entities;

public sealed class MedijskaDatoteka
{
    public int Id { get; set; }
    public string NazivDatoteke { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Sadrzaj { get; set; } = [];
    public long Velicina { get; set; }
    public DateTime KreiranaUtc { get; set; }
    public int KreiraoKorisnikId { get; set; }
    public Korisnik KreiraoKorisnik { get; set; } = null!;
}
