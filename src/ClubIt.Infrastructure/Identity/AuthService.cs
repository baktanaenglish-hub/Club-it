using System.Security.Cryptography;
using ClubIt.Application.Abstractions;
using ClubIt.Application.Common;
using ClubIt.Contracts.Auth;
using ClubIt.Domain.Entities;
using ClubIt.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClubIt.Infrastructure.Identity;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    AppDbContext db,
    JwtTokenService tokens,
    IClock clock) : IAuthService
{
    private const string InviteCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int InviteCodeLength = 8;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new ValidationErrors();
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add(nameof(request.Email), "יש להזין כתובת אימייל.");
        }
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add(nameof(request.Password), "יש להזין סיסמה.");
        }
        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors.Add(nameof(request.DisplayName), "יש להזין שם.");
        }

        var createHousehold = !string.IsNullOrWhiteSpace(request.HouseholdName);
        var joinHousehold = !string.IsNullOrWhiteSpace(request.InviteCode);
        if (createHousehold == joinHousehold)
        {
            errors.Add(nameof(request.HouseholdName), "יש להזין שם למשק בית חדש או קוד הצטרפות למשק בית קיים.");
        }
        errors.ThrowIfAny();

        Household household;
        if (joinHousehold)
        {
            var code = request.InviteCode!.Trim().ToUpperInvariant();
            household = await db.Households.FirstOrDefaultAsync(h => h.InviteCode == code, cancellationToken)
                ?? throw new ValidationException(nameof(request.InviteCode), "קוד ההצטרפות אינו תקין.");
        }
        else
        {
            household = new Household
            {
                Id = Guid.NewGuid(),
                Name = request.HouseholdName!.Trim(),
                InviteCode = await GenerateUniqueInviteCodeAsync(cancellationToken),
                CreatedAtUtc = clock.UtcNow
            };
            // Saved together with the user by UserManager.CreateAsync, so a failed registration leaves nothing behind.
            db.Households.Add(household);
        }

        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            HouseholdId = household.Id
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Identity"] = result.Errors.Select(e => e.Description).ToArray()
            });
        }

        return CreateResponse(user, household);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        const string invalidCredentials = "אימייל או סיסמה שגויים.";

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            throw new UnauthorizedException(invalidCredentials);
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            throw new UnauthorizedException(invalidCredentials);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            throw new UnauthorizedException("החשבון ננעל זמנית עקב ניסיונות התחברות כושלים. נסו שוב מאוחר יותר.");
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            throw new UnauthorizedException(invalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var household = await db.Households.AsNoTracking().FirstAsync(h => h.Id == user.HouseholdId, cancellationToken);
        return CreateResponse(user, household);
    }

    private AuthResponse CreateResponse(ApplicationUser user, Household household)
    {
        var (token, expires) = tokens.CreateToken(user);
        return new AuthResponse(
            token,
            expires,
            new UserDto(user.Id, user.Email!, user.DisplayName, household.Id, household.Name));
    }

    private async Task<string> GenerateUniqueInviteCodeAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var code = RandomNumberGenerator.GetString(InviteCodeAlphabet, InviteCodeLength);
            if (!await db.Households.AnyAsync(h => h.InviteCode == code, cancellationToken))
            {
                return code;
            }
        }
    }
}
