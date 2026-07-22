using Microsoft.EntityFrameworkCore;
using OpenAmp.Domain.Entities;

namespace OpenAmp.Infrastructure.Persistence;

public sealed class OpenAmpDbContext(DbContextOptions<OpenAmpDbContext> options) : DbContext(options)
{
    public DbSet<Korisnik> Korisnici => Set<Korisnik>();
    public DbSet<Uloga> Uloge => Set<Uloga>();
    public DbSet<Instrument> Instrumenti => Set<Instrument>();
    public DbSet<KorisnikInstrument> KorisnikInstrumenti => Set<KorisnikInstrument>();
    public DbSet<Zanr> Zanrovi => Set<Zanr>();
    public DbSet<Bend> Bendovi => Set<Bend>();
    public DbSet<ClanBenda> ClanoviBenda => Set<ClanBenda>();
    public DbSet<PozivnicaBenda> PozivniceBenda => Set<PozivnicaBenda>();
    public DbSet<StatusPozivnice> StatusiPozivnica => Set<StatusPozivnice>();
    public DbSet<Studio> Studiji => Set<Studio>();
    public DbSet<Sala> Sale => Set<Sala>();
    public DbSet<StatusSale> StatusiSala => Set<StatusSale>();
    public DbSet<SalaSlika> SlikeSala => Set<SalaSlika>();
    public DbSet<Oprema> Oprema => Set<Oprema>();
    public DbSet<KategorijaOpreme> KategorijeOpreme => Set<KategorijaOpreme>();
    public DbSet<StatusOpreme> StatusiOpreme => Set<StatusOpreme>();
    public DbSet<Artikal> Artikli => Set<Artikal>();
    public DbSet<KategorijaArtikla> KategorijeArtikala => Set<KategorijaArtikla>();
    public DbSet<StatusArtikla> StatusiArtikala => Set<StatusArtikla>();
    public DbSet<Rezervacija> Rezervacije => Set<Rezervacija>();
    public DbSet<StavkaRezervacije> StavkeRezervacija => Set<StavkaRezervacije>();
    public DbSet<StatusRezervacije> StatusiRezervacija => Set<StatusRezervacije>();
    public DbSet<Recenzija> Recenzije => Set<Recenzija>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpenAmpDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
