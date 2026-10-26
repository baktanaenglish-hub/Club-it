using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ClubIt.Application.Abstractions;
using ClubIt.Application.Common;
using ClubIt.Infrastructure.Identity;

namespace ClubIt.Api.Infrastructure;

/// <summary>Reads the current user from the JWT claims of the request.</summary>
public class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId => ReadGuid(JwtRegisteredClaimNames.Sub, ClaimTypes.NameIdentifier);

    public Guid HouseholdId => ReadGuid(ClubItClaims.HouseholdId);

    private Guid ReadGuid(params string[] claimTypes)
    {
        var principal = httpContextAccessor.HttpContext?.User;
        var value = claimTypes.Select(type => principal?.FindFirstValue(type)).FirstOrDefault(v => v is not null);

        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedException("המשתמש אינו מחובר.");
    }
}
