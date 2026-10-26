using ClubIt.Api.Infrastructure;
using ClubIt.Application.Services;
using ClubIt.Contracts.Clubs;
using Microsoft.AspNetCore.Mvc;

namespace ClubIt.Api.Controllers;

[Route("api/clubs")]
public class ClubsController(CatalogService catalog) : ApiControllerBase
{
    /// <summary>All available clubs.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClubDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await catalog.GetClubsAsync(cancellationToken));
}
