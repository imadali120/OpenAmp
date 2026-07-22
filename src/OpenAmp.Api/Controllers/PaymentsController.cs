using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Api.Authentication;
using OpenAmp.Application.Common;
using OpenAmp.Application.Payments;

namespace OpenAmp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/payments")]
public sealed class PaymentsController(
    ICommandHandler<KreirajPaymentIntentCommand, PaymentIntentDto> handler) : ControllerBase
{
    [HttpPost("reservations/{reservationId:int}/payment-intent")]
    public Task<PaymentIntentDto> CreatePaymentIntent(
        int reservationId,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(
            new KreirajPaymentIntentCommand(reservationId, User.KorisnikId()),
            cancellationToken);
}
