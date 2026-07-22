using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Application.Auth;
using OpenAmp.Application.Payments;
using OpenAmp.Application.Reservations;

namespace OpenAmp.Api.Errors;

public sealed partial class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NeispravniPodaciZaPrijavuException or NeispravanRefreshTokenException =>
                (StatusCodes.Status401Unauthorized, "Autentikacija nije uspjela"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Neautorizovan zahtjev"),
            NedozvoljenaOperacijaException => (StatusCodes.Status403Forbidden, "Operacija nije dozvoljena"),
            EntitetNijePronadjenException or KorisnikNijePronadjenException =>
                (StatusCodes.Status404NotFound, "Resurs nije pronađen"),
            EmailJeZauzetException or TerminNijeDostupanException or KonfliktKonkurentnostiException =>
                (StatusCodes.Status409Conflict, "Konflikt podataka"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Neispravan zahtjev"),
            StripeWebhookPotpisException => (StatusCodes.Status400BadRequest, "Neispravan Stripe webhook"),
            StripeNijeKonfigurisanException => (StatusCodes.Status503ServiceUnavailable, "Stripe nije konfigurisan"),
            PaymentProviderException => (StatusCodes.Status502BadGateway, "Stripe zahtjev nije uspio"),
            _ => (StatusCodes.Status500InternalServerError, "Interna greška servera")
        };

        if (status >= 500)
        {
            LogApiError(logger, exception, status);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "Došlo je do neočekivane greške."
                    : exception.Message,
                Instance = httpContext.Request.Path
            }
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "API request failed with status {StatusCode}")]
    private static partial void LogApiError(ILogger logger, Exception exception, int statusCode);
}
