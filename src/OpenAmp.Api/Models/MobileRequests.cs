using System.ComponentModel.DataAnnotations;

namespace OpenAmp.Api.Models;

public sealed record CreateBandRequest(
    [param: Required, StringLength(150, MinimumLength = 2)] string Naziv,
    [param: Range(1, int.MaxValue)] int ZanrId,
    [param: StringLength(1000)] string? Opis);

public sealed record InviteBandMemberRequest(
    [param: Required, EmailAddress, StringLength(320)] string Email);

public sealed record UpdateBandRequest(
    [param: Required, StringLength(150, MinimumLength = 2)] string Naziv,
    [param: Range(1, int.MaxValue)] int ZanrId,
    [param: StringLength(1000)] string? Opis);

public sealed record RespondBandInvitationRequest(
    bool Prihvati,
    [param: Range(1, int.MaxValue)] int? InstrumentId);

public sealed record UpdateBandMemberRequest(
    [param: Range(1, int.MaxValue)] int? InstrumentId,
    [param: StringLength(100)] string? Uloga);

public sealed record UpdateUserSettingsRequest(
    bool PushNotifikacije,
    bool EmailNotifikacije,
    [param: Required, StringLength(10)] string Jezik,
    bool ProfilJavan);

public sealed record CreateReviewRequest(
    [param: Range(1, 5)] int Ocjena,
    [param: StringLength(2000)] string? Komentar);
