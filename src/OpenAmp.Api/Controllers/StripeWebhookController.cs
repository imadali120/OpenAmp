using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Application.Common;
using OpenAmp.Application.Payments;

namespace OpenAmp.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/webhooks/stripe")]
public sealed class StripeWebhookController(
    ICommandHandler<ObradiStripeWebhookCommand, bool> handler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();
        await handler.HandleAsync(new ObradiStripeWebhookCommand(payload, signature), cancellationToken);
        return Ok(new { received = true });
    }
}
