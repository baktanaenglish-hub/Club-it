using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ClubIt.Infrastructure.Identity;

/// <remarks>Token lifetimes use the real <see cref="TimeProvider"/>, not the business-date <c>IClock</c>.</remarks>
public class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTime ExpiresAtUtc) CreateToken(ApplicationUser user)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expires = now.AddDays(_options.ExpirationDays);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClubItClaims.HouseholdId, user.HouseholdId.ToString())
        };

        var credentials = new SigningCredentials(CreateSigningKey(_options.SigningKey), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public static SymmetricSecurityKey CreateSigningKey(string signingKey) => new(Encoding.UTF8.GetBytes(signingKey));
}
