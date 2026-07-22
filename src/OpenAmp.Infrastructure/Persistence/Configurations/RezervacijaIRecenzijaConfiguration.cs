using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAmp.Domain.Entities;

namespace OpenAmp.Infrastructure.Persistence.Configurations;

internal sealed class RezervacijaConfiguration : IEntityTypeConfiguration<Rezervacija>
{
    public void Configure(EntityTypeBuilder<Rezervacija> builder)
    {
        builder.ToTable("Rezervacije", table =>
        {
            table.HasCheckConstraint("CK_Rezervacije_Termin", "[TerminDoUtc] > [TerminOdUtc]");
            table.HasCheckConstraint("CK_Rezervacije_Cijena", "[UkupnaCijena] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TerminOdUtc).HasPrecision(0);
        builder.Property(x => x.TerminDoUtc).HasPrecision(0);
        builder.Property(x => x.UkupnaCijena).HasPrecision(12, 2);
        builder.Property(x => x.Napomena).HasMaxLength(2000);
        builder.Property(x => x.StripePaymentIntentId).HasMaxLength(255);
        builder.Property(x => x.KreiranaUtc).HasPrecision(0);
        builder.Property(x => x.AzuriranaUtc).HasPrecision(0);
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        // Indeks omogućava SQL Serveru da u SERIALIZABLE transakciji efikasno
        // zaključa raspon termina i spriječi dva istovremena INSERT-a za istu salu.
        builder.HasIndex(x => new { x.SalaId, x.TerminOdUtc, x.TerminDoUtc })
            .HasDatabaseName("IX_Rezervacije_Sala_Termin");
        builder.HasIndex(x => x.StripePaymentIntentId)
            .IsUnique()
            .HasFilter("[StripePaymentIntentId] IS NOT NULL");
        builder.HasOne(x => x.Sala)
            .WithMany(x => x.Rezervacije)
            .HasForeignKey(x => x.SalaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Bend)
            .WithMany(x => x.Rezervacije)
            .HasForeignKey(x => x.BendId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.KreiraoKorisnik)
            .WithMany(x => x.KreiraneRezervacije)
            .HasForeignKey(x => x.KreiraoKorisnikId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Status)
            .WithMany(x => x.Rezervacije)
            .HasForeignKey(x => x.StatusRezervacijeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class StavkaRezervacijeConfiguration : IEntityTypeConfiguration<StavkaRezervacije>
{
    public void Configure(EntityTypeBuilder<StavkaRezervacije> builder)
    {
        builder.ToTable("StavkeRezervacija", table =>
        {
            table.HasCheckConstraint(
                "CK_StavkeRezervacija_Tip",
                "([OpremaId] IS NOT NULL AND [ArtikalId] IS NULL) OR ([OpremaId] IS NULL AND [ArtikalId] IS NOT NULL)");
            table.HasCheckConstraint("CK_StavkeRezervacija_Kolicina", "[Kolicina] > 0");
            table.HasCheckConstraint("CK_StavkeRezervacija_Cijene", "[JedinicnaCijena] >= 0 AND [BrojSati] >= 0 AND [UkupnaCijena] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Naziv).HasMaxLength(200).IsRequired();
        builder.Property(x => x.JedinicnaCijena).HasPrecision(10, 2);
        builder.Property(x => x.BrojSati).HasPrecision(8, 2);
        builder.Property(x => x.UkupnaCijena).HasPrecision(12, 2);
        builder.HasIndex(x => new { x.RezervacijaId, x.OpremaId });
        builder.HasOne(x => x.Rezervacija)
            .WithMany(x => x.Stavke)
            .HasForeignKey(x => x.RezervacijaId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Oprema)
            .WithMany(x => x.StavkeRezervacija)
            .HasForeignKey(x => x.OpremaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Artikal)
            .WithMany(x => x.StavkeRezervacija)
            .HasForeignKey(x => x.ArtikalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class RecenzijaConfiguration : IEntityTypeConfiguration<Recenzija>
{
    public void Configure(EntityTypeBuilder<Recenzija> builder)
    {
        builder.ToTable("Recenzije", table =>
            table.HasCheckConstraint("CK_Recenzije_Ocjena", "[Ocjena] BETWEEN 1 AND 5"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Komentar).HasMaxLength(3000);
        builder.Property(x => x.KreiranaUtc).HasPrecision(0);
        builder.HasIndex(x => new { x.SalaId, x.KreiranaUtc });
        builder.HasIndex(x => x.RezervacijaId)
            .IsUnique()
            .HasFilter("[RezervacijaId] IS NOT NULL");
        builder.HasOne(x => x.Korisnik)
            .WithMany(x => x.Recenzije)
            .HasForeignKey(x => x.KorisnikId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Sala)
            .WithMany(x => x.Recenzije)
            .HasForeignKey(x => x.SalaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Rezervacija)
            .WithOne(x => x.Recenzija)
            .HasForeignKey<Recenzija>(x => x.RezervacijaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
