using ClubIt.Api.Infrastructure;
using ClubIt.Application.Services;
using ClubIt.Contracts.Categories;
using Microsoft.AspNetCore.Mvc;

namespace ClubIt.Api.Controllers;

[Route("api/categories")]
public class CategoriesController(CatalogService catalog) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await catalog.GetCategoriesAsync(cancellationToken));
}
