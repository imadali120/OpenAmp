using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpenAmp.Infrastructure.Persistence;

public sealed class OpenAmpDbContextFactory : IDesignTimeDbContextFactory<OpenAmpDbContext>
{
    public OpenAmpDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("OPENAMP_CONNECTION_STRING")
            ?? "Server=localhost,1433;Database=OpenAmp;User Id=sa;Password=OpenAmp_Dev123!;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<OpenAmpDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(OpenAmpDbContext).Assembly.FullName))
            .Options;

        return new OpenAmpDbContext(options);
    }
}
