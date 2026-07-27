using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAmp.Application.Auth;
using OpenAmp.Application.Common;
using OpenAmp.Application.Desktop;
using OpenAmp.Application.Mobile;
using OpenAmp.Application.Messaging;
using OpenAmp.Application.Payments;
using OpenAmp.Application.Recommendations;
using OpenAmp.Application.Reporting;
using OpenAmp.Application.Reservations;
using OpenAmp.Infrastructure.Auth;
using OpenAmp.Infrastructure.Desktop;
using OpenAmp.Infrastructure.Mobile;
using OpenAmp.Infrastructure.Media;
using OpenAmp.Infrastructure.Messaging;
using OpenAmp.Infrastructure.Payments;
using OpenAmp.Infrastructure.Persistence;
using OpenAmp.Infrastructure.Persistence.Seed;
using OpenAmp.Infrastructure.Recommendations;
using OpenAmp.Infrastructure.Reporting;
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
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<OpenAmpDbContext>());
        services.AddScoped<IKorisnikRepository, KorisnikRepository>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IRezervacijaService, RezervacijaService>();
        services.AddScoped<IMobileExperienceService, MobileExperienceService>();
        services.AddScoped<IMessagePublisher, RabbitMqMessagePublisher>();
        services.AddScoped<IDesktopAdminService, DesktopAdminService>();
        services.AddScoped<IReferenceDataService, ReferenceDataService>();
        services.AddScoped<OpenAmp.Application.Media.IMediaService, MediaService>();
        services.AddScoped<IStripeGateway, StripeGateway>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<DevelopmentDataSeeder>();
        return services;
    }
}
