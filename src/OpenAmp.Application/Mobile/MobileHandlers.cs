using OpenAmp.Application.Common;

namespace OpenAmp.Application.Mobile;

public sealed class PretraziSaleQueryHandler(IMobileExperienceService service)
    : IQueryHandler<PretraziSaleQuery, IReadOnlyCollection<SalaCardDto>>
{
    public Task<IReadOnlyCollection<SalaCardDto>> HandleAsync(
        PretraziSaleQuery query,
        CancellationToken cancellationToken = default) =>
        service.PretraziSaleAsync(query, cancellationToken);
}

public sealed class DohvatiSaluQueryHandler(IMobileExperienceService service)
    : IQueryHandler<DohvatiSaluQuery, SalaDetaljiDto>
{
    public Task<SalaDetaljiDto> HandleAsync(
        DohvatiSaluQuery query,
        CancellationToken cancellationToken = default) =>
        service.DohvatiSaluAsync(query.SalaId, cancellationToken);
}

public sealed class DohvatiMobileSifarnikeQueryHandler(IMobileExperienceService service)
    : IQueryHandler<DohvatiMobileSifarnikeQuery, MobileLookupsDto>
{
    public Task<MobileLookupsDto> HandleAsync(
        DohvatiMobileSifarnikeQuery query,
        CancellationToken cancellationToken = default) =>
        service.DohvatiSifarnikeAsync(cancellationToken);
}

public sealed class DohvatiMojeBendoveQueryHandler(IMobileExperienceService service)
    : IQueryHandler<DohvatiMojeBendoveQuery, IReadOnlyCollection<BendDto>>
{
    public Task<IReadOnlyCollection<BendDto>> HandleAsync(
        DohvatiMojeBendoveQuery query,
        CancellationToken cancellationToken = default) =>
        service.DohvatiBendoveAsync(query.KorisnikId, cancellationToken);
}

public sealed class KreirajBendCommandHandler(IMobileExperienceService service)
    : ICommandHandler<KreirajBendCommand, BendDto>
{
    public Task<BendDto> HandleAsync(
        KreirajBendCommand command,
        CancellationToken cancellationToken = default) =>
        service.KreirajBendAsync(command, cancellationToken);
}

public sealed class PosaljiPozivnicuBendaCommandHandler(IMobileExperienceService service)
    : ICommandHandler<PosaljiPozivnicuBendaCommand, PozivnicaBendaDto>
{
    public Task<PozivnicaBendaDto> HandleAsync(
        PosaljiPozivnicuBendaCommand command,
        CancellationToken cancellationToken = default) =>
        service.PosaljiPozivnicuAsync(command, cancellationToken);
}

public sealed class DohvatiMojeRezervacijeQueryHandler(IMobileExperienceService service)
    : IQueryHandler<DohvatiMojeRezervacijeQuery, IReadOnlyCollection<MobileRezervacijaDto>>
{
    public Task<IReadOnlyCollection<MobileRezervacijaDto>> HandleAsync(
        DohvatiMojeRezervacijeQuery query,
        CancellationToken cancellationToken = default) =>
        service.DohvatiRezervacijeAsync(query.KorisnikId, cancellationToken);
}

public sealed class DohvatiProfilPregledQueryHandler(IMobileExperienceService service)
    : IQueryHandler<DohvatiProfilPregledQuery, ProfilPregledDto>
{
    public Task<ProfilPregledDto> HandleAsync(
        DohvatiProfilPregledQuery query,
        CancellationToken cancellationToken = default) =>
        service.DohvatiProfilAsync(query.KorisnikId, cancellationToken);
}
