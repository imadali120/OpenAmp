using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAmp.Domain.Entities;

namespace OpenAmp.Infrastructure.Persistence.Configurations;

internal sealed class MedijskaDatotekaConfiguration : IEntityTypeConfiguration<MedijskaDatoteka>
{
    public void Configure(EntityTypeBuilder<MedijskaDatoteka> builder)
    {
        builder.ToTable("MedijskeDatoteke", table =>
            table.HasCheckConstraint("CK_MedijskeDatoteke_Velicina", "[Velicina] > 0 AND [Velicina] <= 5242880"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NazivDatoteke).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Sadrzaj).IsRequired();
        builder.Property(x => x.KreiranaUtc).HasPrecision(0);
        builder.HasIndex(x => new { x.KreiraoKorisnikId, x.KreiranaUtc });
        builder.HasOne(x => x.KreiraoKorisnik)
            .WithMany()
            .HasForeignKey(x => x.KreiraoKorisnikId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
