using ClubIt.Api.Infrastructure;
using ClubIt.Application.Abstractions;
using ClubIt.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClubIt.Api.Controllers;

[Route("api/auth")]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Auth)]
public class AuthController(IAuthService auth) : ApiControllerBase
{
    /// <summary>Registers a user and creates a new household, or joins an existing one with an invite code.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken) =>
        await auth.RegisterAsync(request, cancellationToken);

    /// <summary>Signs in and returns a JWT.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken) =>
        await auth.LoginAsync(request, cancellationToken);
}
