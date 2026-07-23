using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAmp.Domain.Entities;

namespace OpenAmp.Infrastructure.Persistence.Configurations;

internal sealed class KorisnikConfiguration : IEntityTypeConfiguration<Korisnik>
{
    public void Configure(EntityTypeBuilder<Korisnik> builder)
    {
        builder.ToTable("Korisnici");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Username).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Ime).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Prezime).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Telefon).HasMaxLength(30);
        builder.Property(x => x.FotografijaUrl).HasMaxLength(2048);
        builder.Property(x => x.StripeCustomerId).HasMaxLength(255);
        builder.Property(x => x.KreiranUtc).HasPrecision(0);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasIndex(x => x.StripeCustomerId).IsUnique().HasFilter("[StripeCustomerId] IS NOT NULL");
        builder.HasOne(x => x.Uloga)
            .WithMany(x => x.Korisnici)
            .HasForeignKey(x => x.UlogaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ProfilnaSlika)
            .WithMany()
            .HasForeignKey(x => x.ProfilnaSlikaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class KorisnikInstrumentConfiguration : IEntityTypeConfiguration<KorisnikInstrument>
{
    public void Configure(EntityTypeBuilder<KorisnikInstrument> builder)
    {
        builder.ToTable("KorisnikInstrumenti");
        builder.HasKey(x => new { x.KorisnikId, x.InstrumentId });
        builder.HasOne(x => x.Korisnik)
            .WithMany(x => x.Instrumenti)
            .HasForeignKey(x => x.KorisnikId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Instrument)
            .WithMany(x => x.Korisnici)
            .HasForeignKey(x => x.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PostavkeKorisnikaConfiguration : IEntityTypeConfiguration<PostavkeKorisnika>
{
    public void Configure(EntityTypeBuilder<PostavkeKorisnika> builder)
    {
        builder.ToTable("PostavkeKorisnika");
        builder.HasKey(x => x.KorisnikId);
        builder.Property(x => x.Jezik).HasMaxLength(10).IsRequired();
        builder.Property(x => x.AzuriraneUtc).HasPrecision(0);
        builder.HasOne(x => x.Korisnik)
            .WithOne(x => x.Postavke)
            .HasForeignKey<PostavkeKorisnika>(x => x.KorisnikId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class OmiljenaSalaConfiguration : IEntityTypeConfiguration<OmiljenaSala>
{
    public void Configure(EntityTypeBuilder<OmiljenaSala> builder)
    {
        builder.ToTable("OmiljeneSale");
        builder.HasKey(x => new { x.KorisnikId, x.SalaId });
        builder.Property(x => x.KreiranaUtc).HasPrecision(0);
        builder.HasIndex(x => new { x.KorisnikId, x.KreiranaUtc });
        builder.HasOne(x => x.Korisnik)
            .WithMany(x => x.OmiljeneSale)
            .HasForeignKey(x => x.KorisnikId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Sala)
            .WithMany(x => x.Favoriti)
            .HasForeignKey(x => x.SalaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokeni", table =>
            table.HasCheckConstraint("CK_RefreshTokeni_Datum", "[IsticeUtc] > [KreiranUtc]"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ZamijenjenTokenHash).HasMaxLength(64);
        builder.Property(x => x.KreiranSaIpAdrese).HasMaxLength(64);
        builder.Property(x => x.KreiranUtc).HasPrecision(0);
        builder.Property(x => x.IsticeUtc).HasPrecision(0);
        builder.Property(x => x.OpozvanUtc).HasPrecision(0);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.KorisnikId, x.IsticeUtc });
        builder.HasOne(x => x.Korisnik)
            .WithMany(x => x.RefreshTokeni)
            .HasForeignKey(x => x.KorisnikId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class BendConfiguration : IEntityTypeConfiguration<Bend>
{
    public void Configure(EntityTypeBuilder<Bend> builder)
    {
        builder.ToTable("Bendovi");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Naziv).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Opis).HasMaxLength(2000);
        builder.Property(x => x.FotografijaUrl).HasMaxLength(2048);
        builder.Property(x => x.KreiranUtc).HasPrecision(0);
        builder.HasIndex(x => x.Naziv);
        builder.HasOne(x => x.Osnivac)
            .WithMany(x => x.OsnovaniBendovi)
            .HasForeignKey(x => x.OsnivacId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Zanr)
            .WithMany(x => x.Bendovi)
            .HasForeignKey(x => x.ZanrId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Fotografija)
            .WithMany()
            .HasForeignKey(x => x.FotografijaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class ClanBendaConfiguration : IEntityTypeConfiguration<ClanBenda>
{
    public void Configure(EntityTypeBuilder<ClanBenda> builder)
    {
        builder.ToTable("ClanoviBenda");
        builder.HasKey(x => new { x.BendId, x.KorisnikId });
        builder.Property(x => x.UlogaUBendu).HasMaxLength(100);
        builder.Property(x => x.DatumPridruzivanjaUtc).HasPrecision(0);
        builder.HasOne(x => x.Bend)
            .WithMany(x => x.Clanovi)
            .HasForeignKey(x => x.BendId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Korisnik)
            .WithMany(x => x.Clanstva)
            .HasForeignKey(x => x.KorisnikId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Instrument)
            .WithMany(x => x.ClanoviBendova)
            .HasForeignKey(x => x.InstrumentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PozivnicaBendaConfiguration : IEntityTypeConfiguration<PozivnicaBenda>
{
    public void Configure(EntityTypeBuilder<PozivnicaBenda> builder)
    {
        builder.ToTable("PozivniceBenda", table =>
            table.HasCheckConstraint("CK_PozivniceBenda_Datum", "[IsticeUtc] > [KreiranaUtc]"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Username).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Kod).HasMaxLength(100).IsRequired();
        builder.Property(x => x.KreiranaUtc).HasPrecision(0);
        builder.Property(x => x.IsticeUtc).HasPrecision(0);
        builder.Property(x => x.OdgovorenaUtc).HasPrecision(0);
        builder.HasIndex(x => x.Kod).IsUnique();
        builder.HasIndex(x => new { x.BendId, x.Username, x.StatusPozivniceId });
        builder.HasOne(x => x.Bend)
            .WithMany(x => x.Pozivnice)
            .HasForeignKey(x => x.BendId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.PozvaoKorisnik)
            .WithMany(x => x.PoslanePozivnice)
            .HasForeignKey(x => x.PozvaoKorisnikId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PozvaniKorisnik)
            .WithMany(x => x.PrimljenePozivnice)
            .HasForeignKey(x => x.PozvaniKorisnikId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Status)
            .WithMany(x => x.Pozivnice)
            .HasForeignKey(x => x.StatusPozivniceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
