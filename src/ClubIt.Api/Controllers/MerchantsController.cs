using ClubIt.Api.Infrastructure;
using ClubIt.Application.Services;
using ClubIt.Contracts.Merchants;
using Microsoft.AspNetCore.Mvc;

namespace ClubIt.Api.Controllers;

[Route("api/merchants")]
public class MerchantsController(MerchantService merchants) : ApiControllerBase
{
    /// <summary>Merchants matching a name or alias, for autocomplete.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MerchantDto>>> Search([FromQuery] string? q, CancellationToken cancellationToken) =>
        Ok(await merchants.SearchAsync(q, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MerchantDto>> Get(int id, CancellationToken cancellationToken) =>
        await merchants.GetAsync(id, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<MerchantDto>> Create(CreateMerchantRequest request, CancellationToken cancellationToken)
    {
        var created = await merchants.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Renames a merchant, changes its category or replaces its aliases.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MerchantDto>> Update(int id, UpdateMerchantRequest request, CancellationToken cancellationToken) =>
        await merchants.UpdateAsync(id, request, cancellationToken);

    [HttpGet("{id:int}/branches")]
    public async Task<ActionResult<IReadOnlyList<BranchDto>>> GetBranches(int id, CancellationToken cancellationToken) =>
        Ok(await merchants.GetBranchesAsync(id, cancellationToken));

    [HttpPost("{id:int}/branches")]
    public async Task<ActionResult<BranchDto>> CreateBranch(int id, CreateBranchRequest request, CancellationToken cancellationToken)
    {
        var created = await merchants.CreateBranchAsync(id, request, cancellationToken);
        return Created($"/api/merchants/{id}/branches", created);
    }
}
