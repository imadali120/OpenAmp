using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAmp.Domain.Entities;
using OpenAmp.Infrastructure.Persistence.Seed;

namespace OpenAmp.Infrastructure.Persistence.Configurations;

internal sealed class StudioConfiguration : IEntityTypeConfiguration<Studio>
{
    public void Configure(EntityTypeBuilder<Studio> builder)
    {
        builder.ToTable("Studiji");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Naziv).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Opis).HasMaxLength(3000);
        builder.Property(x => x.Adresa).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Grad).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Telefon).HasMaxLength(30);
        builder.Property(x => x.Email).HasMaxLength(320);
        builder.HasIndex(x => new { x.Grad, x.Naziv });
        builder.HasOne(x => x.Vlasnik)
            .WithMany(x => x.Studiji)
            .HasForeignKey(x => x.VlasnikId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasData(OpenAmpSeed.Studiji);
    }
}

internal sealed class SalaConfiguration : IEntityTypeConfiguration<Sala>
{
    public void Configure(EntityTypeBuilder<Sala> builder)
    {
        builder.ToTable("Sale", table =>
        {
            table.HasCheckConstraint("CK_Sale_Kapacitet", "[Kapacitet] > 0");
            table.HasCheckConstraint("CK_Sale_CijenaPoSatu", "[CijenaPoSatu] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Naziv).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CijenaPoSatu).HasPrecision(10, 2);
        builder.Property(x => x.Opis).HasMaxLength(3000);
        builder.Property(x => x.Akustika).HasMaxLength(1000);
        builder.Property(x => x.GeografskaSirina).HasPrecision(9, 6);
        builder.Property(x => x.GeografskaDuzina).HasPrecision(9, 6);
        builder.HasIndex(x => new { x.StudioId, x.Naziv }).IsUnique();
        builder.HasOne(x => x.Studio)
            .WithMany(x => x.Sale)
            .HasForeignKey(x => x.StudioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Status)
            .WithMany(x => x.Sale)
            .HasForeignKey(x => x.StatusSaleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasData(OpenAmpSeed.Sale);
    }
}

internal sealed class SalaSlikaConfiguration : IEntityTypeConfiguration<SalaSlika>
{
    public void Configure(EntityTypeBuilder<SalaSlika> builder)
    {
        builder.ToTable("SlikeSala");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.AlternativniTekst).HasMaxLength(250);
        builder.HasIndex(x => new { x.SalaId, x.Redoslijed }).IsUnique();
        builder.HasOne(x => x.Sala)
            .WithMany(x => x.Galerija)
            .HasForeignKey(x => x.SalaId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasData(OpenAmpSeed.SlikeSala);
    }
}

internal sealed class OpremaConfiguration : IEntityTypeConfiguration<Oprema>
{
    public void Configure(EntityTypeBuilder<Oprema> builder)
    {
        builder.ToTable("Oprema", table =>
            table.HasCheckConstraint("CK_Oprema_CijenaNajma", "[CijenaNajmaPoSatu] >= 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InventarskiBroj).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Naziv).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Opis).HasMaxLength(2000);
        builder.Property(x => x.SerijskiBroj).HasMaxLength(100);
        builder.Property(x => x.CijenaNajmaPoSatu).HasPrecision(10, 2);
        builder.Property(x => x.Napomena).HasMaxLength(1000);
        builder.HasIndex(x => x.InventarskiBroj).IsUnique();
        builder.HasIndex(x => new { x.KategorijaOpremeId, x.StatusOpremeId, x.SalaId });
        builder.HasOne(x => x.Kategorija)
            .WithMany(x => x.Oprema)
            .HasForeignKey(x => x.KategorijaOpremeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Status)
            .WithMany(x => x.Oprema)
            .HasForeignKey(x => x.StatusOpremeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Sala)
            .WithMany(x => x.Oprema)
            .HasForeignKey(x => x.SalaId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasData(OpenAmpSeed.Oprema);
    }
}

internal sealed class ArtikalConfiguration : IEntityTypeConfiguration<Artikal>
{
    public void Configure(EntityTypeBuilder<Artikal> builder)
    {
        builder.ToTable("Artikli", table =>
        {
            table.HasCheckConstraint("CK_Artikli_Zalihe", "[KolicinaNaStanju] >= 0 AND [MinimalnaZaliha] >= 0");
            table.HasCheckConstraint("CK_Artikli_Cijena", "[CijenaKupovine] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InventarskiBroj).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Naziv).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Opis).HasMaxLength(2000);
        builder.Property(x => x.CijenaKupovine).HasPrecision(10, 2);
        builder.HasIndex(x => x.InventarskiBroj).IsUnique();
        builder.HasIndex(x => new { x.StudioId, x.KolicinaNaStanju });
        builder.HasOne(x => x.Kategorija)
            .WithMany(x => x.Artikli)
            .HasForeignKey(x => x.KategorijaArtiklaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Status)
            .WithMany(x => x.Artikli)
            .HasForeignKey(x => x.StatusArtiklaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Studio)
            .WithMany(x => x.Artikli)
            .HasForeignKey(x => x.StudioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasData(OpenAmpSeed.Artikli);
    }
}
