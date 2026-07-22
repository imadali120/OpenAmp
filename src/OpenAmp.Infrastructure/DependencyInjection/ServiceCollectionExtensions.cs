using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAmp.Application.Reservations;
using OpenAmp.Infrastructure.Persistence;
using OpenAmp.Infrastructure.Reservations;

namespace OpenAmp.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenAmpInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<OpenAmpDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddScoped<IRezervacijaService, RezervacijaService>();
        return services;
    }
}
