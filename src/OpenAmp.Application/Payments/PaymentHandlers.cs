using OpenAmp.Application.Common;

namespace OpenAmp.Application.Payments;

public sealed class KreirajPaymentIntentCommandHandler(IPaymentService service)
    : ICommandHandler<KreirajPaymentIntentCommand, PaymentIntentDto>
{
    public Task<PaymentIntentDto> HandleAsync(
        KreirajPaymentIntentCommand command,
        CancellationToken cancellationToken = default) =>
        service.KreirajPaymentIntentAsync(command.RezervacijaId, command.KorisnikId, cancellationToken);
}

public sealed class ObradiStripeWebhookCommandHandler(IPaymentService service)
    : ICommandHandler<ObradiStripeWebhookCommand, bool>
{
    public async Task<bool> HandleAsync(
        ObradiStripeWebhookCommand command,
        CancellationToken cancellationToken = default)
    {
        await service.ObradiWebhookAsync(command.Payload, command.StripeSignature, cancellationToken);
        return true;
    }
}
