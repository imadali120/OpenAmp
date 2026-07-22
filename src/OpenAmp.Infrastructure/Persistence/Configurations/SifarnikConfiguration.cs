using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenAmp.Domain.Entities;
using OpenAmp.Infrastructure.Persistence.Seed;

namespace OpenAmp.Infrastructure.Persistence.Configurations;

internal abstract class SifarnikConfiguration<T> : IEntityTypeConfiguration<T> where T : class
{
    protected abstract string TableName { get; }

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey("Id");
        builder.Property("Kod").HasMaxLength(50).IsRequired();
        builder.Property("Naziv").HasMaxLength(100).IsRequired();
        builder.HasIndex("Kod").IsUnique();
    }
}

internal sealed class UlogaConfiguration : SifarnikConfiguration<Uloga>
{
    protected override string TableName => "Uloge";
    public override void Configure(EntityTypeBuilder<Uloga> builder)
    {
        base.Configure(builder);
        builder.HasData(OpenAmpSeed.Uloge);
    }
}

internal sealed class ZanrConfiguration : SifarnikConfiguration<Zanr>
{
    protected override string TableName => "Zanrovi";
    public override void Configure(EntityTypeBuilder<Zanr> builder)
    {
        base.Configure(builder);
        builder.HasData(OpenAmpSeed.Zanrovi);
    }
}

internal sealed class InstrumentConfiguration : SifarnikConfiguration<Instrument>
{
    protected override string TableName => "Instrumenti";
    public override void Configure(EntityTypeBuilder<Instrument> builder)
    {
        base.Configure(builder);
        builder.HasData(OpenAmpSeed.Instrumenti);
    }
}

internal sealed class StatusSaleConfiguration : SifarnikConfiguration<StatusSale>
{
    protected override string TableName => "StatusiSala";
    public override void Configure(EntityTypeBuilder<StatusSale> builder)
    {
        base.Configure(builder);
        builder.HasData(OpenAmpSeed.StatusiSala);
    }
}

internal sealed class KategorijaOpremeConfiguration : SifarnikConfiguration<KategorijaOpreme>
{
    protected override string TableName => "KategorijeOpreme";
    public override void Configure(EntityTypeBuilder<KategorijaOpreme> builder)
    {
        base.Configure(builder);
        builder.HasData(OpenAmpSeed.KategorijeOpreme);
    }
}

internal sealed class StatusOpremeConfiguration : SifarnikConfiguration<StatusOpreme>
{
    protected override string TableName => "StatusiOpreme";
    public override void Configure(EntityTypeBuilder<StatusOpreme> builder)
    {
        base.Configure(builder);
        builder.HasData(OpenAmpSeed.StatusiOpreme);
    }
}

internal sealed class KategorijaArtiklaConfiguration : SifarnikConfiguration<KategorijaArtikla>
{
    protected override string TableName => "KategorijeArtikala";
    public override void Configure(EntityTypeBuilder<KategorijaArtikla> builder)
    {
        base.Configure(builder);
        builder.HasData(OpenAmpSeed.KategorijeArtikala);
    }
}

internal sealed class StatusArtiklaConfiguration : SifarnikConfiguration<StatusArtikla>
{
    protected override string TableName => "StatusiArtikala";
    public override void Configure(EntityTypeBuilder<StatusArtikla> builder)
    {
        base.Configure(builder);
        builder.HasData(OpenAmpSeed.StatusiArtikala);
    }
}

internal sealed class StatusRezervacijeConfiguration : SifarnikConfiguration<StatusRezervacije>
{
    protected override string TableName => "StatusiRezervacija";
    public override void Configure(EntityTypeBuilder<StatusRezervacije> builder)
    {
        base.Configure(builder);
        builder.HasData(OpenAmpSeed.StatusiRezervacija);
    }
}

internal sealed class StatusPozivniceConfiguration : SifarnikConfiguration<StatusPozivnice>
{
    protected override string TableName => "StatusiPozivnica";
    public override void Configure(EntityTypeBuilder<StatusPozivnice> builder)
    {
        base.Configure(builder);
        builder.HasData(OpenAmpSeed.StatusiPozivnica);
    }
}
