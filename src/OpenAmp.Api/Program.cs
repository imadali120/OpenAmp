using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenAmp.Api.Authentication;
using OpenAmp.Api.Errors;
using OpenAmp.Application.Auth;
using OpenAmp.Application.Common;
using OpenAmp.Application.Mobile;
using OpenAmp.Application.Payments;
using OpenAmp.Application.Reservations;
using OpenAmp.Infrastructure.DependencyInjection;
using OpenAmp.Infrastructure.Payments;
using OpenAmp.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("OpenAmp")
    ?? throw new InvalidOperationException("Connection string 'OpenAmp' nije konfigurisan.");

builder.Services.AddOpenAmpInfrastructure(connectionString);
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.SectionName));
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(x => x.SigningKey.Length >= 32, "JWT signing key mora imati najmanje 32 znaka.")
    .Validate(x => x.AccessTokenMinutes is >= 5 and <= 120, "JWT trajanje mora biti između 5 i 120 minuta.")
    .ValidateOnStart();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT konfiguracija nedostaje.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();

builder.Services.AddScoped<ICommandHandler<RegisterCommand, AuthResponseDto>, RegisterCommandHandler>();
builder.Services.AddScoped<ICommandHandler<LoginCommand, AuthResponseDto>, LoginCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RefreshTokenCommand, AuthResponseDto>, RefreshTokenCommandHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiKorisnikaQuery, KorisnikDto>, DohvatiKorisnikaQueryHandler>();
builder.Services.AddScoped<ICommandHandler<AzurirajProfilCommand, KorisnikDto>, AzurirajProfilCommandHandler>();
builder.Services.AddScoped<ICommandHandler<PromijeniLozinkuCommand, bool>, PromijeniLozinkuCommandHandler>();
builder.Services.AddScoped<ICommandHandler<KreirajRezervacijuCommand, RezervacijaDto>, KreirajRezervacijuCommandHandler>();
builder.Services.AddScoped<ICommandHandler<IzmijeniRezervacijuCommand, RezervacijaDto>, IzmijeniRezervacijuCommandHandler>();
builder.Services.AddScoped<ICommandHandler<OtkaziRezervacijuCommand, OtkazivanjeRezultatDto>, OtkaziRezervacijuCommandHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiRezervacijuQuery, RezervacijaDto>, DohvatiRezervacijuQueryHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiSlobodneTermineQuery, IReadOnlyCollection<SlobodanTerminDto>>, DohvatiSlobodneTermineQueryHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiOtkazivanjePregledQuery, OtkazivanjePregledDto>, DohvatiOtkazivanjePregledQueryHandler>();
builder.Services.AddScoped<ICommandHandler<KreirajPaymentIntentCommand, PaymentIntentDto>, KreirajPaymentIntentCommandHandler>();
builder.Services.AddScoped<ICommandHandler<ObradiStripeWebhookCommand, bool>, ObradiStripeWebhookCommandHandler>();
builder.Services.AddScoped<IQueryHandler<PretraziSaleQuery, IReadOnlyCollection<SalaCardDto>>, PretraziSaleQueryHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiSaluQuery, SalaDetaljiDto>, DohvatiSaluQueryHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiMobileSifarnikeQuery, MobileLookupsDto>, DohvatiMobileSifarnikeQueryHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiMojeBendoveQuery, IReadOnlyCollection<BendDto>>, DohvatiMojeBendoveQueryHandler>();
builder.Services.AddScoped<ICommandHandler<KreirajBendCommand, BendDto>, KreirajBendCommandHandler>();
builder.Services.AddScoped<ICommandHandler<PosaljiPozivnicuBendaCommand, PozivnicaBendaDto>, PosaljiPozivnicuBendaCommandHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiPrimljenePozivniceQuery, IReadOnlyCollection<PrimljenaPozivnicaBendaDto>>, DohvatiPrimljenePozivniceQueryHandler>();
builder.Services.AddScoped<ICommandHandler<OdgovoriNaPozivnicuBendaCommand, PrimljenaPozivnicaBendaDto>, OdgovoriNaPozivnicuBendaCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AzurirajBendCommand, BendDto>, AzurirajBendCommandHandler>();
builder.Services.AddScoped<ICommandHandler<AzurirajClanaBendaCommand, BendDto>, AzurirajClanaBendaCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UkloniClanaBendaCommand, BendDto>, UkloniClanaBendaCommandHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiMojeRezervacijeQuery, IReadOnlyCollection<MobileRezervacijaDto>>, DohvatiMojeRezervacijeQueryHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiProfilPregledQuery, ProfilPregledDto>, DohvatiProfilPregledQueryHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiOmiljeneSaleQuery, IReadOnlyCollection<int>>, DohvatiOmiljeneSaleQueryHandler>();
builder.Services.AddScoped<ICommandHandler<PostaviOmiljenuSaluCommand, bool>, PostaviOmiljenuSaluCommandHandler>();
builder.Services.AddScoped<IQueryHandler<DohvatiKorisnickePostavkeQuery, KorisnickePostavkeDto>, DohvatiKorisnickePostavkeQueryHandler>();
builder.Services.AddScoped<ICommandHandler<AzurirajKorisnickePostavkeCommand, KorisnickePostavkeDto>, AzurirajKorisnickePostavkeCommandHandler>();
builder.Services.AddScoped<ICommandHandler<KreirajRecenzijuCommand, RecenzijaSaleDto>, KreirajRecenzijuCommandHandler>();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks().AddDbContextCheck<OpenAmpDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenAmp API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = []
    });
});

var app = builder.Build();
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<OpenAmpDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

public partial class Program;
