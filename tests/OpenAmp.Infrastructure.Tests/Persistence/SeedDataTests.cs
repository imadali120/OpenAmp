using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OpenAmp.Infrastructure.Persistence;

namespace OpenAmp.Infrastructure.Tests.Persistence;

public sealed class SeedDataTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly OpenAmpDbContext _context;

    public SeedDataTests()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder<OpenAmpDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new OpenAmpDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task SeedSadrziSveTriZahtijevaneUloge()
    {
        var kodovi = await _context.Uloge
            .OrderBy(x => x.Id)
            .Select(x => x.Kod)
            .ToArrayAsync();

        Assert.Equal(["ADMIN", "ZAPOSLENIK", "MUZICAR"], kodovi);
    }

    [Fact]
    public async Task SeedSadrziTestneSaleOpremuIArtikle()
    {
        Assert.True(await _context.Sale.CountAsync() >= 3);
        Assert.True(await _context.Oprema.CountAsync() >= 3);
        Assert.True(await _context.Artikli.CountAsync() >= 3);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
