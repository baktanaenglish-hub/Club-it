using System.Net;

namespace ClubIt.Mobile.Core.Services;

/// <summary>An API error with a message that can be shown to the user as-is.</summary>
public class ApiException(
    HttpStatusCode statusCode,
    string message,
    IReadOnlyDictionary<string, string[]>? errors = null,
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors ?? new Dictionary<string, string[]>();

    /// <summary>The main message followed by every field error, one per line.</summary>
    public string UserMessage =>
        Errors.Count == 0
            ? Message
            : string.Join(Environment.NewLine, Errors.SelectMany(e => e.Value).Distinct());
}
