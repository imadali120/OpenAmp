using System.ComponentModel.DataAnnotations;

namespace OpenAmp.Api.Models;

public sealed record CreateBandRequest(
    [param: Required, StringLength(150, MinimumLength = 2)] string Naziv,
    [param: Range(1, int.MaxValue)] int ZanrId,
    [param: StringLength(1000)] string? Opis);

public sealed record InviteBandMemberRequest(
    [param: Required, EmailAddress, StringLength(320)] string Email);
