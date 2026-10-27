using System.Text;

namespace ClubIt.Domain.Common;

/// <summary>
/// Normalizes merchant names and aliases for search: lower-case, without whitespace
/// or punctuation, so "Shufer Sal", "שופר-סל" and "שופרסל" all compare equal.
/// </summary>
public static class NameNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }

        return builder.ToString();
    }
}
