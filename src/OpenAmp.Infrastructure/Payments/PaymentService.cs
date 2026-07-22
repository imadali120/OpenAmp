using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using OpenAmp.Application.Payments;
using OpenAmp.Application.Reservations;
using OpenAmp.Domain.Entities;
using OpenAmp.Infrastructure.Persistence;
using Stripe;

namespace OpenAmp.Infrastructure.Payments;

public sealed class PaymentService(
    OpenAmpDbContext dbContext,
    IStripeGateway stripeGateway,
    IOptions<StripeOptions> options,
    TimeProvider timeProvider) : IPaymentService
{
    private readonly StripeOptions _options = options.Value;

    public async Task<PaymentIntentDto> KreirajPaymentIntentAsync(
        int rezervacijaId,
        int korisnikId,
        CancellationToken cancellationToken = default)
    {
        var rezervacija = await dbContext.Rezervacije
            .Include(x => x.Status)
            .Include(x => x.KreiraoKorisnik)
            .SingleOrDefaultAsync(x => x.Id == rezervacijaId, cancellationToken)
            ?? throw new EntitetNijePronadjenException($"Rezervacija {rezervacijaId} nije pronađena.");
        if (rezervacija.KreiraoKorisnikId != korisnikId)
        {
            throw new NedozvoljenaOperacijaException("Nemate pristup ovoj rezervaciji.");
        }

        if (rezervacija.Status.Kod != "NA_CEKANJU")
        {
            throw new NedozvoljenaOperacijaException("Payment Intent se kreira samo za rezervaciju koja čeka plaćanje.");
        }

        var iznos = StripeGateway.UNajmanjuJedinicu(rezervacija.UkupnaCijena);
        var korisnik = rezervacija.KreiraoKorisnik;
        if (string.IsNullOrWhiteSpace(korisnik.StripeCustomerId))
        {
            korisnik.StripeCustomerId = await stripeGateway.KreirajKupcaAsync(
                korisnik.Id,
                korisnik.Email,
                $"{korisnik.Ime} {korisnik.Prezime}",
                cancellationToken);
        }
        var revizija = Convert.ToHexString(rezervacija.RowVersion);
        var intent = await stripeGateway.KreirajIliAzurirajPaymentIntentAsync(
            rezervacija.StripePaymentIntentId,
            iznos,
            stripeGateway.Valuta,
            korisnik.StripeCustomerId,
            rezervacija.Id,
            $"openamp-rezervacija-{rezervacija.Id}-payment-{revizija}",
            cancellationToken);
        rezervacija.StripePaymentIntentId = intent.Id;
        rezervacija.AzuriranaUtc = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);
        var customerSession = await stripeGateway.KreirajCustomerSessionAsync(
            korisnik.StripeCustomerId,
            cancellationToken);
        return new PaymentIntentDto(
            intent.Id,
            intent.ClientSecret,
            iznos,
            stripeGateway.Valuta,
            korisnik.StripeCustomerId,
            customerSession);
    }

    public async Task ObradiWebhookAsync(
        string payload,
        string stripeSignature,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            throw new StripeNijeKonfigurisanException();
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, stripeSignature, _options.WebhookSecret);
        }
        catch (StripeException exception)
        {
            throw new StripeWebhookPotpisException(exception.Message);
        }

        if (await dbContext.StripeWebhookDogadjaji.AnyAsync(x => x.Id == stripeEvent.Id, cancellationToken))
        {
            return;
        }

        if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded
            && stripeEvent.Data.Object is PaymentIntent paymentIntent)
        {
            var rezervacija = await dbContext.Rezervacije
                .Include(x => x.Status)
                .SingleOrDefaultAsync(x => x.StripePaymentIntentId == paymentIntent.Id, cancellationToken)
                ?? throw new EntitetNijePronadjenException(
                    $"Rezervacija za Stripe PaymentIntent {paymentIntent.Id} nije pronađena.");
            var ocekivaniIznos = StripeGateway.UNajmanjuJedinicu(rezervacija.UkupnaCijena);
            if (paymentIntent.AmountReceived < ocekivaniIznos
                || !string.Equals(paymentIntent.Currency, stripeGateway.Valuta, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Stripe uplata ne odgovara iznosu ili valuti rezervacije.");
            }

            rezervacija.Status = await dbContext.StatusiRezervacija
                .SingleAsync(x => x.Kod == "PLACENA", cancellationToken);
            rezervacija.StatusRezervacijeId = rezervacija.Status.Id;
            rezervacija.AzuriranaUtc = timeProvider.GetUtcNow().UtcDateTime;
        }

        dbContext.StripeWebhookDogadjaji.Add(new StripeWebhookDogadjaj
        {
            Id = stripeEvent.Id,
            Tip = stripeEvent.Type,
            ObradjenUtc = timeProvider.GetUtcNow().UtcDateTime
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (JeUniqueConstraint(exception))
        {
            // Drugi webhook worker je već atomarski obradio isti Stripe event.
        }
    }

    private static bool JeUniqueConstraint(Exception exception)
    {
        for (var trenutni = exception; trenutni is not null; trenutni = trenutni.InnerException)
        {
            if (trenutni is SqlException { Number: 2601 or 2627 })
            {
                return true;
            }
        }

        return false;
    }
}
