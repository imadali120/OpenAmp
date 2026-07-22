using OpenAmp.Application.Common;

namespace OpenAmp.Application.Reservations;

public sealed class KreirajRezervacijuCommandHandler(IRezervacijaService service)
    : ICommandHandler<KreirajRezervacijuCommand, RezervacijaDto>
{
    public Task<RezervacijaDto> HandleAsync(
        KreirajRezervacijuCommand command,
        CancellationToken cancellationToken = default) => service.KreirajAsync(command, cancellationToken);
}

public sealed class IzmijeniRezervacijuCommandHandler(IRezervacijaService service)
    : ICommandHandler<IzmijeniRezervacijuCommand, RezervacijaDto>
{
    public Task<RezervacijaDto> HandleAsync(
        IzmijeniRezervacijuCommand command,
        CancellationToken cancellationToken = default) => service.PromijeniTerminAsync(command, cancellationToken);
}

public sealed class OtkaziRezervacijuCommandHandler(IRezervacijaService service)
    : ICommandHandler<OtkaziRezervacijuCommand, OtkazivanjeRezultatDto>
{
    public Task<OtkazivanjeRezultatDto> HandleAsync(
        OtkaziRezervacijuCommand command,
        CancellationToken cancellationToken = default) => service.OtkaziAsync(command, cancellationToken);
}

public sealed class DohvatiRezervacijuQueryHandler(IRezervacijaService service)
    : IQueryHandler<DohvatiRezervacijuQuery, RezervacijaDto>
{
    public Task<RezervacijaDto> HandleAsync(
        DohvatiRezervacijuQuery query,
        CancellationToken cancellationToken = default) =>
        service.DohvatiAsync(query.RezervacijaId, query.KorisnikId, cancellationToken);
}

public sealed class DohvatiSlobodneTermineQueryHandler(IRezervacijaService service)
    : IQueryHandler<DohvatiSlobodneTermineQuery, IReadOnlyCollection<SlobodanTerminDto>>
{
    public Task<IReadOnlyCollection<SlobodanTerminDto>> HandleAsync(
        DohvatiSlobodneTermineQuery query,
        CancellationToken cancellationToken = default) =>
        service.DohvatiSlobodneTermineAsync(query, cancellationToken);
}
