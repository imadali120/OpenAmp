using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAmp.Api.Authentication;
using OpenAmp.Application.Media;

namespace OpenAmp.Api.Controllers;

[ApiController]
[Route("api/images")]
public sealed class ImagesController(IMediaService mediaService) : ControllerBase
{
    private const long MaxUploadSize = 5 * 1024 * 1024;

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var slika = await mediaService.DohvatiAsync(id, cancellationToken);
        if (slika is null)
        {
            return NotFound();
        }

        Response.Headers.CacheControl = "public,max-age=86400";
        return File(slika.Sadrzaj, slika.ContentType, enableRangeProcessing: true);
    }

    [Authorize]
    [HttpPost("profile")]
    [RequestSizeLimit(MaxUploadSize)]
    public async Task<ActionResult<UploadSlikeDto>> UploadProfile(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        var result = await mediaService.PostaviProfilnuSlikuAsync(
            User.KorisnikId(),
            await ProcitajAsync(file, cancellationToken),
            cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPost("bands/{bandId:int}")]
    [RequestSizeLimit(MaxUploadSize)]
    public async Task<ActionResult<UploadSlikeDto>> UploadBand(
        int bandId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        var result = await mediaService.PostaviSlikuBendaAsync(
            User.KorisnikId(),
            bandId,
            await ProcitajAsync(file, cancellationToken),
            cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPost("studios/{studioId:int}")]
    [RequestSizeLimit(MaxUploadSize)]
    public async Task<ActionResult<UploadSlikeDto>> UploadStudio(
        int studioId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        var result = await mediaService.PostaviSlikuStudijaAsync(
            User.KorisnikId(),
            studioId,
            await ProcitajAsync(file, cancellationToken),
            cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPost("halls/{hallId:int}")]
    [RequestSizeLimit(MaxUploadSize)]
    public async Task<ActionResult<UploadSlikeDto>> UploadHall(
        int hallId,
        [FromForm] IFormFile file,
        [FromForm] string? alternativeText,
        CancellationToken cancellationToken)
    {
        var result = await mediaService.DodajSlikuSaleAsync(
            User.KorisnikId(),
            hallId,
            await ProcitajAsync(file, cancellationToken),
            alternativeText,
            cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    private static async Task<NovaSlikaDto> ProcitajAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length is <= 0 or > MaxUploadSize)
        {
            throw new ArgumentException("Slika mora biti manja od 5 MB.");
        }

        await using var stream = new MemoryStream((int)file.Length);
        await file.CopyToAsync(stream, cancellationToken);
        return new NovaSlikaDto(file.FileName, file.ContentType, stream.ToArray());
    }
}
