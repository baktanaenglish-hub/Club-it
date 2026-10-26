using ClubIt.Api.Infrastructure;
using ClubIt.Application.Services;
using ClubIt.Contracts.Clubs;
using ClubIt.Contracts.Households;
using Microsoft.AspNetCore.Mvc;

namespace ClubIt.Api.Controllers;

[Route("api/household")]
public class HouseholdController(HouseholdService households) : ApiControllerBase
{
    /// <summary>The current user's household, including the invite code for a partner.</summary>
    [HttpGet]
    public async Task<ActionResult<HouseholdDto>> Get(CancellationToken cancellationToken) =>
        await households.GetHouseholdAsync(cancellationToken);

    /// <summary>The clubs the household marked in settings.</summary>
    [HttpGet("clubs")]
    public async Task<ActionResult<IReadOnlyList<ClubDto>>> GetClubs(CancellationToken cancellationToken) =>
        Ok(await households.GetClubsAsync(cancellationToken));

    /// <summary>Replaces the household's club selection.</summary>
    [HttpPut("clubs")]
    public async Task<ActionResult<IReadOnlyList<ClubDto>>> UpdateClubs(UpdateHouseholdClubsRequest request, CancellationToken cancellationToken) =>
        Ok(await households.UpdateClubsAsync(request, cancellationToken));
}
