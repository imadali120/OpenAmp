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

public sealed class DohvatiPrimljenePozivniceQueryHandler(IMobileExperienceService service)
    : IQueryHandler<DohvatiPrimljenePozivniceQuery, IReadOnlyCollection<PrimljenaPozivnicaBendaDto>>
{
    public Task<IReadOnlyCollection<PrimljenaPozivnicaBendaDto>> HandleAsync(
        DohvatiPrimljenePozivniceQuery query,
        CancellationToken cancellationToken = default) =>
        service.DohvatiPrimljenePozivniceAsync(query.KorisnikId, cancellationToken);
}

public sealed class OdgovoriNaPozivnicuBendaCommandHandler(IMobileExperienceService service)
    : ICommandHandler<OdgovoriNaPozivnicuBendaCommand, PrimljenaPozivnicaBendaDto>
{
    public Task<PrimljenaPozivnicaBendaDto> HandleAsync(
        OdgovoriNaPozivnicuBendaCommand command,
        CancellationToken cancellationToken = default) =>
        service.OdgovoriNaPozivnicuAsync(command, cancellationToken);
}

public sealed class AzurirajBendCommandHandler(IMobileExperienceService service)
    : ICommandHandler<AzurirajBendCommand, BendDto>
{
    public Task<BendDto> HandleAsync(AzurirajBendCommand command, CancellationToken cancellationToken = default) =>
        service.AzurirajBendAsync(command, cancellationToken);
}

public sealed class AzurirajClanaBendaCommandHandler(IMobileExperienceService service)
    : ICommandHandler<AzurirajClanaBendaCommand, BendDto>
{
    public Task<BendDto> HandleAsync(
        AzurirajClanaBendaCommand command,
        CancellationToken cancellationToken = default) =>
        service.AzurirajClanaAsync(command, cancellationToken);
}

public sealed class UkloniClanaBendaCommandHandler(IMobileExperienceService service)
    : ICommandHandler<UkloniClanaBendaCommand, BendDto>
{
    public Task<BendDto> HandleAsync(
        UkloniClanaBendaCommand command,
        CancellationToken cancellationToken = default) =>
        service.UkloniClanaAsync(command, cancellationToken);
}

public sealed class DohvatiOmiljeneSaleQueryHandler(IMobileExperienceService service)
    : IQueryHandler<DohvatiOmiljeneSaleQuery, IReadOnlyCollection<int>>
{
    public Task<IReadOnlyCollection<int>> HandleAsync(
        DohvatiOmiljeneSaleQuery query,
        CancellationToken cancellationToken = default) =>
        service.DohvatiOmiljeneSaleAsync(query.KorisnikId, cancellationToken);
}

public sealed class PostaviOmiljenuSaluCommandHandler(IMobileExperienceService service)
    : ICommandHandler<PostaviOmiljenuSaluCommand, bool>
{
    public Task<bool> HandleAsync(
        PostaviOmiljenuSaluCommand command,
        CancellationToken cancellationToken = default) =>
        service.PostaviOmiljenuSaluAsync(command, cancellationToken);
}

public sealed class DohvatiKorisnickePostavkeQueryHandler(IMobileExperienceService service)
    : IQueryHandler<DohvatiKorisnickePostavkeQuery, KorisnickePostavkeDto>
{
    public Task<KorisnickePostavkeDto> HandleAsync(
        DohvatiKorisnickePostavkeQuery query,
        CancellationToken cancellationToken = default) =>
        service.DohvatiPostavkeAsync(query.KorisnikId, cancellationToken);
}

public sealed class AzurirajKorisnickePostavkeCommandHandler(IMobileExperienceService service)
    : ICommandHandler<AzurirajKorisnickePostavkeCommand, KorisnickePostavkeDto>
{
    public Task<KorisnickePostavkeDto> HandleAsync(
        AzurirajKorisnickePostavkeCommand command,
        CancellationToken cancellationToken = default) =>
        service.AzurirajPostavkeAsync(command, cancellationToken);
}

public sealed class KreirajRecenzijuCommandHandler(IMobileExperienceService service)
    : ICommandHandler<KreirajRecenzijuCommand, RecenzijaSaleDto>
{
    public Task<RecenzijaSaleDto> HandleAsync(
        KreirajRecenzijuCommand command,
        CancellationToken cancellationToken = default) =>
        service.KreirajRecenzijuAsync(command, cancellationToken);
}
