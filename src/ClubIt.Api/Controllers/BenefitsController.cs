using ClubIt.Api.Infrastructure;
using ClubIt.Application.Services;
using ClubIt.Contracts.Benefits;
using Microsoft.AspNetCore.Mvc;

namespace ClubIt.Api.Controllers;

[Route("api/benefits")]
public class BenefitsController(BenefitService benefits) : ApiControllerBase
{
    /// <summary>Valid benefits for a store name or alias, from the household's clubs. Optionally limited to one branch.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<BenefitDto>>> Search(
        [FromQuery] string? q, [FromQuery] int? branchId, CancellationToken cancellationToken) =>
        Ok(await benefits.SearchAsync(q, branchId, cancellationToken));

    /// <summary>Valid outing benefits (movies, shows, attractions), optionally for a single club.</summary>
    [HttpGet("entertainment")]
    public async Task<ActionResult<IReadOnlyList<BenefitDto>>> Entertainment(
        [FromQuery] int? clubId, CancellationToken cancellationToken) =>
        Ok(await benefits.GetEntertainmentAsync(clubId, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BenefitDto>> Get(int id, CancellationToken cancellationToken) =>
        await benefits.GetAsync(id, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<BenefitDto>> Create(SaveBenefitRequest request, CancellationToken cancellationToken)
    {
        var created = await benefits.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BenefitDto>> Update(int id, SaveBenefitRequest request, CancellationToken cancellationToken) =>
        await benefits.UpdateAsync(id, request, cancellationToken);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await benefits.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
