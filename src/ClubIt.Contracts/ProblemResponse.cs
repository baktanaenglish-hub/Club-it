namespace ClubIt.Contracts;

/// <summary>Subset of RFC 7807 problem details returned by the API on errors.</summary>
public record ProblemResponse(string? Title, int? Status, string? Detail, IDictionary<string, string[]>? Errors);
