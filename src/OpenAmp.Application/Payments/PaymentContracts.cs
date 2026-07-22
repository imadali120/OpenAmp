using OpenAmp.Application.Common;

namespace OpenAmp.Application.Payments;

public sealed record PaymentIntentDto(
    string PaymentIntentId,
    string ClientSecret,
    long IznosUNajmanjojJedinici,
    string Valuta);

public sealed record StripePaymentIntentResult(string Id, string ClientSecret);
public sealed record StripeRefundResult(string Id, decimal Iznos);

public sealed record KreirajPaymentIntentCommand(int RezervacijaId, int KorisnikId)
    : ICommand<PaymentIntentDto>;

public sealed record ObradiStripeWebhookCommand(string Payload, string StripeSignature)
    : ICommand<bool>;

public interface IPaymentService
{
    Task<PaymentIntentDto> KreirajPaymentIntentAsync(
        int rezervacijaId,
        int korisnikId,
        CancellationToken cancellationToken = default);

    Task ObradiWebhookAsync(
        string payload,
        string stripeSignature,
        CancellationToken cancellationToken = default);
}

public interface IStripeGateway
{
    string Valuta { get; }

    Task<StripePaymentIntentResult> KreirajIliAzurirajPaymentIntentAsync(
        string? postojeciPaymentIntentId,
        long iznosUNajmanjojJedinici,
        string valuta,
        int rezervacijaId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<StripeRefundResult> RefundirajAsync(
        string paymentIntentId,
        decimal iznos,
        string valuta,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task OtkaziPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);
}

public sealed class StripeNijeKonfigurisanException()
    : InvalidOperationException("Stripe nije konfigurisan. Postavite Stripe__SecretKey i Stripe__WebhookSecret.");

public sealed class StripeWebhookPotpisException(string message) : InvalidOperationException(message);

public sealed class PaymentProviderException(string message, Exception innerException)
    : InvalidOperationException(message, innerException);
