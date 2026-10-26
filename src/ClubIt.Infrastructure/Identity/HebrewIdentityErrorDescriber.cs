using Microsoft.AspNetCore.Identity;

namespace ClubIt.Infrastructure.Identity;

/// <summary>Hebrew versions of the Identity errors a user can hit while registering.</summary>
public class HebrewIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = $"כתובת האימייל {email} כבר רשומה." };

    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = $"כתובת האימייל {userName} כבר רשומה." };

    public override IdentityError InvalidEmail(string? email) =>
        new() { Code = nameof(InvalidEmail), Description = "כתובת האימייל אינה תקינה." };

    public override IdentityError InvalidUserName(string? userName) =>
        new() { Code = nameof(InvalidUserName), Description = "כתובת האימייל אינה תקינה." };

    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"הסיסמה חייבת להכיל לפחות {length} תווים." };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "הסיסמה חייבת להכיל לפחות ספרה אחת." };

    public override IdentityError PasswordRequiresLower() =>
        new() { Code = nameof(PasswordRequiresLower), Description = "הסיסמה חייבת להכיל לפחות אות אחת." };

    public override IdentityError PasswordRequiresUpper() =>
        new() { Code = nameof(PasswordRequiresUpper), Description = "הסיסמה חייבת להכיל לפחות אות גדולה אחת." };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "הסיסמה חייבת להכיל לפחות תו מיוחד אחד." };
}
