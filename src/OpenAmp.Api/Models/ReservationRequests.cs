using System.ComponentModel.DataAnnotations;

namespace OpenAmp.Api.Models;

public sealed record ReservationItemRequest(
    int? OpremaId,
    int? ArtikalId,
    [param: Range(1, 1000)] int Kolicina);

public sealed record CreateReservationRequest(
    [param: Range(1, int.MaxValue)] int SalaId,
    [param: Range(1, int.MaxValue)] int BendId,
    DateTime TerminOdUtc,
    DateTime TerminDoUtc,
    [param: StringLength(2000)] string? Napomena,
    IReadOnlyCollection<ReservationItemRequest>? Stavke);

public sealed record UpdateReservationRequest(
    DateTime TerminOdUtc,
    DateTime TerminDoUtc,
    [param: Required] string RowVersion);

public sealed record CancelReservationRequest(
    [param: Required] string RowVersion,
    [param: StringLength(1000)] string? Razlog);
