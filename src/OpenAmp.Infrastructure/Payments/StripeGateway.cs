using Microsoft.Extensions.Options;
using OpenAmp.Application.Payments;
using Stripe;

namespace OpenAmp.Infrastructure.Payments;

public sealed class StripeGateway(IOptions<StripeOptions> options) : IStripeGateway
{
    private readonly StripeOptions _options = options.Value;

    public string Valuta => _options.Currency.ToLowerInvariant();

    public async Task<StripePaymentIntentResult> KreirajIliAzurirajPaymentIntentAsync(
        string? postojeciPaymentIntentId,
        long iznosUNajmanjojJedinici,
        string valuta,
        int rezervacijaId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = KreirajClient();
            var service = new PaymentIntentService(client);
            PaymentIntent intent;
            if (postojeciPaymentIntentId is null)
            {
                intent = await service.CreateAsync(
                    new PaymentIntentCreateOptions
                    {
                        Amount = iznosUNajmanjojJedinici,
                        Currency = valuta,
                        AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                        Description = $"OpenAmp rezervacija #{rezervacijaId}",
                        Metadata = new Dictionary<string, string>
                        {
                            ["rezervacija_id"] = rezervacijaId.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        }
                    },
                    new RequestOptions { IdempotencyKey = idempotencyKey },
                    cancellationToken);
            }
            else
            {
                intent = await service.UpdateAsync(
                    postojeciPaymentIntentId,
                    new PaymentIntentUpdateOptions { Amount = iznosUNajmanjojJedinici },
                    new RequestOptions { IdempotencyKey = idempotencyKey },
                    cancellationToken);
            }

            return new StripePaymentIntentResult(intent.Id, intent.ClientSecret);
        }
        catch (StripeException exception)
        {
            throw new PaymentProviderException("Stripe PaymentIntent zahtjev nije uspio.", exception);
        }
    }

    public async Task<StripeRefundResult> RefundirajAsync(
        string paymentIntentId,
        decimal iznos,
        string valuta,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _ = valuta;
            var service = new RefundService(KreirajClient());
            var refund = await service.CreateAsync(
                new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Amount = UNajmanjuJedinicu(iznos),
                    Reason = "requested_by_customer",
                    Metadata = new Dictionary<string, string> { ["openamp_reason"] = "reservation_cancelled" }
                },
                new RequestOptions { IdempotencyKey = idempotencyKey },
                cancellationToken);
            return new StripeRefundResult(refund.Id, refund.Amount / 100m);
        }
        catch (StripeException exception)
        {
            throw new PaymentProviderException("Stripe refund zahtjev nije uspio.", exception);
        }
    }

    public async Task OtkaziPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService(KreirajClient());
            await service.CancelAsync(paymentIntentId, cancellationToken: cancellationToken);
        }
        catch (StripeException exception)
        {
            throw new PaymentProviderException("Stripe otkazivanje PaymentIntenta nije uspjelo.", exception);
        }
    }

    private StripeClient KreirajClient()
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new StripeNijeKonfigurisanException();
        }

        return new StripeClient(_options.SecretKey);
    }

    internal static long UNajmanjuJedinicu(decimal iznos) =>
        checked((long)decimal.Round(iznos * 100m, 0, MidpointRounding.AwayFromZero));
}
